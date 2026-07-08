# Random Passwort Generator

This application will create tough passwords. CSPRPNG is utilized to create random characters. 

No dependencies required before using it. The executable is already in native mashine code and self-contained.
See "Releases" on the right.

This application is compiled into native machine code (AoT compilation) rather than using IL. This ensures more efficiency. The application is self-contained and doesn't need any pre-installed dependencies in order to run.

## What is CSPRNG and why using it?
A CSPRNG (cryptographically secure pseudorandom number generator) still runs on a mathematical algorithm, but it is seeded, and often continuously reseeded, with entropy gathered from unpredictable sources such as mouse movement, keyboard timing, etc.. This makes its output computationally infeasible to predict, even if someone knows the algorithm.

A plain PRNG (often loosely called "RNG")is not cryptographically secure. It produces a fully deterministic sequence from its algorithm and seed. If a hacker knows which algorithm was used and can figure out or guess the seed, for example if it was seeded from something predictable like the system clock, they can reproduce the entire output sequence, including any password generated from it. The real weakness isn't the password's length, it's that the seed's possible values are limited enough to search or guess.

## Done optimizations

Although this project is small, I did performance optimizations to improve my skills in C#.

So, what did I do?

play this music FIRST. While this music keeps playing, you can continue reading this text.

Music:   https://www.youtube.com/watch?v=-mtcfkWDbOU

What? You don't know pannenkoek2012 / UncommentatedPannen  ??

## Checking out byte allocations and binary sizes

I don't intend to sound misleading. There are some weird things going on under the hood in .NET which I don't fully understand.

I made this same project in C++ and Rust. Their compiled binary sizes were very small:
* C++: 220 KB (Linux)
* Rust: 480 KB (Linux)
* C#:
* * 1.3 MB (former size on Windows)
  * 1.6 MB (former size on Linux)
  * 710 KB (Windows, current binary size),
  * 970 KB (Linux, current binary size)

There is a setting in the `.csproj` file that creates a `.mstat` in `obj/` after compilation. It can be used to check out which resources
(classes, functions etc.) create how many bytes. I noticed, among others, that `System.String` and `System.Console` had the highest amount of byte allocations.
I don't know why. 

## Replacing `System.Console` with native terminal functions

As the title implies, I replaced `System.Console` with native terminal functions. Check out `interop/`. There are two files:
* `linux_terminal.cs`
* `windows_terminal.cs`

Both files simply import native OS functionalities for their own terminal. That way, I don't have to rely on `System.Console`, which saves a lot of bytes.

## Mostly avoiding `System.String`

The `.mstat` file revealed that `System.String` wasted most amount of bytes. I mostly avoided using `System.String`. Instead, I used most of the time:
* `Span<T>`
* `ReadOnlySpan<T>`
* `char[]`
* `byte[]` (a byte is an unsigned int with a size of 2^8 bytes. As such, it already represents a character, when converted to ASCII)
* `stackalloc`

etc..

I have no definitive answer here. But I suspect the reason why `System.String` wasted so many bytes was likely because it carries over many `Exception` messages, which are also included during compile- and runtime. I did find those message in the `.mstat` file, too, although I never explicitly used those `Exception` messages. This explains why there is an extra setting for the `.csproj` file to trim out those messages. I think it was this setting:

```
    <StackTraceSupport>false</StackTraceSupport>
```
After enabling this setting along with several others, the compiled binary became somewhat smaller.

About the `<StackTraceSupport>false</StackTraceSupport>` setting:

It doesn't mean those exception messages will never appear, which would be actually problematic. Don't remember the details, but instead, key numbers will be output for specific exceptions, warnings, etc.. Those key numbers are already documented on the website of Microsoft for .NET development, so you can easily find out the message.

Anyway, after mostly replacing `string`, the compiled binary size became much smaller.

## Extra settings in the `.csproj`

There are various settings in the `.csproj` to trim out unnecessary things, etc.. They help to reduce the overall binary size after compilation. Check out the project `.csproj` file.
