import fire
import asyncio
import sys
import io

async def delayed_echo_gen():
    for line in sys.stdin:
        await asyncio.sleep(1)
        yield line

def test():
    return delayed_echo_gen()

if __name__ == '__main__':
    value = fire.Fire()
    async def main():
        async for line in value:
            print(line)
    asyncio.run(main())
