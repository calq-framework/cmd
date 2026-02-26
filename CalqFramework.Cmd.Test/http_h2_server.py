import ssl
import socket
import h2.connection
import h2.events
import h2.config
import time

def run_h2_tls_server():
    context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    context.load_cert_chain(certfile='cert.pem', keyfile='key.pem')
    context.set_alpn_protocols(['h2'])

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('0.0.0.0', 8443))
    sock.listen(5)
    print("Listening on https://localhost:8443 (HTTP/2 TLS)")

    while True:
        conn_tcp, addr = sock.accept()
        conn = context.wrap_socket(conn_tcp, server_side=True)

        negotiated = conn.selected_alpn_protocol()
        if negotiated != 'h2':
            print(f"Client did not negotiate h2 (got {negotiated})")
            conn.close()
            continue

        config = h2.config.H2Configuration(client_side=False, header_encoding='utf-8')
        h2_conn = h2.connection.H2Connection(config=config)
        h2_conn.initiate_connection()
        conn.sendall(h2_conn.data_to_send())

        stream_data = {}

        while True:
            data = conn.recv(65535)
            if not data:
                break

            events = h2_conn.receive_data(data)
            for event in events:
                if isinstance(event, h2.events.RequestReceived):
                    stream_id = event.stream_id
                    stream_data[stream_id] = b""
                    print(f"Headers received on stream {stream_id}")

                elif isinstance(event, h2.events.DataReceived):
                    h2_conn.acknowledge_received_data(event.flow_controlled_length, event.stream_id)
                    stream_data[event.stream_id] += event.data
                    conn.sendall(h2_conn.data_to_send())

                elif isinstance(event, h2.events.StreamEnded):
                    stream_id = event.stream_id
                    body = stream_data[stream_id]
                    print(f"Full request body on stream {stream_id}: {body!r}")

                    h2_conn.send_headers(stream_id, [
                        (':status', '200'),
                        ('content-type', 'text/plain'),
                    ])
                    conn.sendall(h2_conn.data_to_send())

                    chunks = [body[i:i+6] for i in range(0, len(body), 6)]

                    for i, chunk in enumerate(chunks):
                        print(f"Sending chunk: {chunk}")
                        h2_conn.send_data(stream_id, chunk, end_stream=False)
                        conn.sendall(h2_conn.data_to_send())

                        if i == 2:
                            # Reset after second chunk
                            time.sleep(1) # TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
                            h2_conn.reset_stream(stream_id, error_code=128)
                            conn.sendall(h2_conn.data_to_send())
                            print(f"RST_STREAM sent on stream {stream_id}")
                            break

                    # End stream properly
                    h2_conn.send_data(stream_id, b"", end_stream=True)
                    conn.sendall(h2_conn.data_to_send())
                    print("Stream ended cleanly.")

        conn.close()

run_h2_tls_server()