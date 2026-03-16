import fire

def add(x: int, y: int):
    return x + y

def upper(msg: str = "hello"):
    return msg.upper()

if __name__ == "__main__":
    fire.Fire()
