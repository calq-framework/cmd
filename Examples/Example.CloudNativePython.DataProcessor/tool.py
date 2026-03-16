import fire
import sys
import os
import threading

def process_chunk():
    data = sys.stdin.read().strip()
    if not data:
        return "Empty chunk"
    return f"Processed: {data.upper()} [PID: {os.getpid()}, TID: {threading.get_ident()}]"

if __name__ == "__main__":
    fire.Fire()
