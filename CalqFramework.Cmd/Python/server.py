# -*- coding: utf-8 -*-
"""
Modified asyncio-server.py
~~~~~~~~~~~~~~~~~
The MIT License (MIT)

Copyright (c) 2015-2020 Cory Benfield and contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

https://github.com/python-hyper/h2/blob/99fb0ca799ac54dbcd9380b2e96b11fb434ac45b/examples/asyncio/asyncio-server.py


A fully-functional HTTP/2 server using asyncio. Requires Python 3.5+.

This example demonstrates handling requests with bodies, as well as handling
those without. In particular, it demonstrates the fact that DataReceived may
be called multiple times, and that applications must handle that possibility.
"""
import asyncio
import io
import json
import ssl
import collections
import traceback
import zlib
from typing import List, Tuple

from h2.config import H2Configuration
from h2.connection import H2Connection
from h2.events import (
    ConnectionTerminated, DataReceived, RemoteSettingsChanged,
    RequestReceived, StreamEnded, StreamReset, WindowUpdated
)
from h2.errors import ErrorCodes
from h2.exceptions import ProtocolError, StreamClosedError
from h2.settings import SettingCodes

import fire
import types
import contextvars
import sys
import builtins
import threading

fire.core._PrintResult = lambda component_trace, verbose=False, serialize=None: None

class SysWithLocalStdIn(types.ModuleType):
    def __init__(self, name, doc=None):
        super().__init__(name, doc)
        self._calq_cmd_stdin_local = contextvars.ContextVar('_calq_cmd_stdin_local')
        self._calq_cmd_stdin_local.set(sys.stdin)

    @property
    def stdin(self):
        return self._calq_cmd_stdin_local.get()

patched_sys = SysWithLocalStdIn(sys.__name__, sys.__doc__)
patched_sys.__dict__.update(sys.__dict__)
sys.modules['sys'] = patched_sys
builtins.sys = patched_sys

import sys

sys.path.append('./')
import test_tool

class H2Protocol(asyncio.Protocol):
    # Configurable integer range for exception hash codes
    ERROR_HASH_RANGE = (256, 0xFFFFFFFF)

    class RequestData:
        def __init__(self, headers, data):
            self.headers = headers
            self.data = data

    def __init__(self):
        config = H2Configuration(client_side=False, header_encoding='utf-8')
        self.conn = H2Connection(config=config)
        self.transport = None
        self.stream_data = {}
        self.stream_task = {}
        self.flow_control_futures = {}

    def connection_made(self, transport: asyncio.Transport):
        self.transport = transport
        self.conn.initiate_connection()
        self.transport.write(self.conn.data_to_send())

    def connection_lost(self, exc):
        for future in self.flow_control_futures.values():
            future.cancel()
        self.flow_control_futures = {}

    def data_received(self, data: bytes):
        try:
            events = self.conn.receive_data(data)
        except ProtocolError as e:
            self.transport.write(self.conn.data_to_send())
            self.transport.close()
        else:
            self.transport.write(self.conn.data_to_send())
            for event in events:
                if isinstance(event, RequestReceived):
                    self.request_received(event.headers, event.stream_id)
                elif isinstance(event, DataReceived):
                    self.receive_data(
                        event.data, event.flow_controlled_length, event.stream_id
                    )
                elif isinstance(event, StreamEnded):
                    self.stream_complete(event.stream_id)
                elif isinstance(event, ConnectionTerminated):
                    self.transport.close()
                elif isinstance(event, StreamReset):
                    self.stream_task[event.stream_id].cancel()
                    self.stream_reset(event.stream_id)
                elif isinstance(event, WindowUpdated):
                    self.window_updated(event.stream_id, event.delta)
                elif isinstance(event, RemoteSettingsChanged):
                    if SettingCodes.INITIAL_WINDOW_SIZE in event.changed_settings:
                        self.window_updated(None, 0)

                self.transport.write(self.conn.data_to_send())

    def request_received(self, headers: List[Tuple[str, str]], stream_id: int):
        headers = collections.OrderedDict(headers)
        method = headers[':method']

        # Store off the request data.
        request_data = H2Protocol.RequestData(headers, io.BytesIO())
        self.stream_data[stream_id] = request_data

    def stream_complete(self, stream_id: int):
        """
        When a stream is complete, we can send our response.
        """
        try:
            request_data = self.stream_data[stream_id]
        except KeyError:
            # Just return, we probably 405'd this already
            return

        headers = request_data.headers
        body = request_data.data.getvalue().decode('utf-8')
        sys._calq_cmd_stdin_local.set(io.StringIO(body))

        response_headers = (
            (':status', '200'),
            ('content-type', 'text/plain'),
            #('content-length', str(len(data))),
            #('server', 'asyncio-h2-calq-cmd'),
        )
        self.conn.send_headers(stream_id, response_headers)

        value = fire.Fire(test_tool, command=headers["script"])
        if isinstance(value, types.AsyncGeneratorType):
            self.stream_task[stream_id] = asyncio.ensure_future(self.send_stream_data(value, stream_id))
        elif isinstance(value, str):
            self.stream_task[stream_id] = asyncio.ensure_future(self.send_data(value.encode("utf8"), stream_id))
        elif isinstance(value, bytes):
            self.stream_task[stream_id] = asyncio.ensure_future(self.send_data(value, stream_id))
        else:
            self.stream_task[stream_id] = asyncio.ensure_future(self.send_data(str(value).encode("utf8"), stream_id))

    async def abort_stream(self, stream_id, error_code):
        # TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
        await asyncio.sleep(1.0)
        self.conn.reset_stream(stream_id, error_code=error_code)
        self.transport.write(self.conn.data_to_send())

    def receive_data(self, data: bytes, flow_controlled_length: int, stream_id: int):
        """
        We've received some data on a stream. If that stream is one we're
        expecting data on, save it off (and account for the received amount of
        data in flow control so that the client can send more data).
        Otherwise, reset the stream.
        """
        try:
            stream_data = self.stream_data[stream_id]
        except KeyError:
            asyncio.create_task(self.abort_stream(stream_id, ErrorCodes.PROTOCOL_ERROR))
        else:
            stream_data.data.write(data)
            self.conn.acknowledge_received_data(flow_controlled_length, stream_id)

    def stream_reset(self, stream_id):
        """
        A stream reset was sent. Stop sending data.
        """
        if stream_id in self.flow_control_futures:
            future = self.flow_control_futures.pop(stream_id)
            future.cancel()

    async def send_data(self, data: bytes, stream_id: int):
        """
        Send data according to the flow control rules.
        """
        while data:
            while self.conn.local_flow_control_window(stream_id) < 1:
                try:
                    await self.wait_for_flow_control(stream_id)
                except asyncio.CancelledError:
                    return

            chunk_size = min(
                self.conn.local_flow_control_window(stream_id),
                len(data),
                self.conn.max_outbound_frame_size,
            )

            try:
                self.conn.send_data(
                    stream_id,
                    data[:chunk_size],
                    end_stream=(chunk_size == len(data))
                )
            except (StreamClosedError, ProtocolError):
                # The stream got closed and we didn't get told. We're done
                # here.
                break

            self.transport.write(self.conn.data_to_send())
            data = data[chunk_size:]

    async def send_stream_data(self, data_gen, stream_id):
        """
        Send data from an async generator according to flow control rules.
        """
        try:
            async for chunk in data_gen:
                data = chunk if isinstance(chunk, bytes) else chunk.encode('utf-8')

                while data:
                    while self.conn.local_flow_control_window(stream_id) < 1:
                        try:
                            await self.wait_for_flow_control(stream_id)
                        except asyncio.CancelledError:
                            return

                    chunk_size = min(
                        self.conn.local_flow_control_window(stream_id),
                        len(data),
                        self.conn.max_outbound_frame_size,
                    )

                    try:
                        self.conn.send_data(
                            stream_id,
                            data[:chunk_size],
                            end_stream=False  # Can't know if this is last chunk yet
                        )
                    except (StreamClosedError, ProtocolError):
                        break

                    self.transport.write(self.conn.data_to_send())
                    data = data[chunk_size:]

            # After all chunks, signal end of stream
            self.conn.send_data(stream_id, b'', end_stream=True)
            self.transport.write(self.conn.data_to_send())

        except (StreamClosedError, ProtocolError):
            # Ignore standard H2 stream closure/errors
            pass
        except Exception:
            # Reset stream on any exception with a hash of the stack trace
            tb = traceback.format_exc()
            checksum = zlib.crc32(tb.encode('utf-8'))
            
            # Map hash to configurable integer range
            start, end = self.ERROR_HASH_RANGE
            if end > start:
                error_code = start + (checksum % (end - start))
            else:
                error_code = start
                
            asyncio.create_task(self.abort_stream(stream_id, error_code))

    async def wait_for_flow_control(self, stream_id):
        """
        Waits for a Future that fires when the flow control window is opened.
        """
        f = asyncio.Future()
        self.flow_control_futures[stream_id] = f
        await f

    def window_updated(self, stream_id, delta):
        """
        A window update frame was received. Unblock some number of flow control
        Futures.
        """
        if stream_id and stream_id in self.flow_control_futures:
            f = self.flow_control_futures.pop(stream_id)
            f.set_result(delta)
        elif not stream_id:
            for f in self.flow_control_futures.values():
                f.set_result(delta)

            self.flow_control_futures = {}

ssl_context = ssl.create_default_context(ssl.Purpose.CLIENT_AUTH)
ssl_context.options |= (
    ssl.OP_NO_TLSv1 | ssl.OP_NO_TLSv1_1 | ssl.OP_NO_COMPRESSION
)
ssl_context.load_cert_chain(certfile='cert.pem', keyfile='key.pem')
ssl_context.set_alpn_protocols(["h2"])

async def main():
    # Each client connection will create a new protocol instance
    server = await asyncio.get_event_loop().create_server(H2Protocol, '127.0.0.1', 8443, ssl=ssl_context)
    
    # Serve requests until Ctrl+C is pressed
    print('Serving on {}'.format(server.sockets[0].getsockname()))
    try:
        await server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        # Close the server
        server.close()
        await server.wait_closed()

asyncio.run(main())
