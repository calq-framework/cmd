import asyncio
import sys
import io

async def test():
    for line in sys.stdin:
        await asyncio.sleep(1)
        yield line

if __name__ == "__main__":
    import fire
    fire.core._PrintResult = lambda component_trace, verbose=False, serialize=None: None
    value = fire.Fire()
    async def main():
        async for part in value:
            print(part, end="")
    asyncio.run(main())
