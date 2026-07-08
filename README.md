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
The main inspiration doing the optimizations was because of this repo: [nikouu/TinyWorldle](https://github.com/nikouu/TinyWordle)

So, what did I do?

play this music FIRST. While this music keeps playing, you can continue reading this text.

Music:   https://www.youtube.com/watch?v=-mtcfkWDbOU

What? You don't know pannenkoek2012 / UncommentatedPannen  ??

## Checking out byte allocations and binary sizes

I don't intend to sound misleading. There are some weird things going on under the hood in .NET which I don't fully understand yet.

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
Mind you, the high number of allocated bytes I saw isn't bad. It's just noticeably high.

Checking out the `.mstat` file made me realize that strings (and other things) are expensive and this explains why developers, depending on their projects, goals and target platforms, may not want to use strings. Not saying though strings are bad and should be avoided. That would be inconvinient. 

If however there is a very big project, then maybe optimizations should be considered for better efficiency and performance. In the project 'TinyWordle' (see [nikouu/TinyWorldle](https://github.com/nikouu/TinyWordle)), nikouu managed to reduce the initial byte size from 62091 KB to a whopping 680 KB.

I will continue using strings just fine unless there is an important reason to write more efficient instructions. Heck, you can even write efficient string instructions e.g. using `string.Create()` and pass `Span` to the method. 

In reality however: frequent optimizations relying on low-level instructions and (perhaps) native functionality imports isn't always ideal for everybody, especially for many other projects.

## Replacing `System.Console` with native terminal functions

As the title implies, I replaced `System.Console` with native terminal functions. Check out `interop/`. There are two files:
* `linux_terminal.cs`
* `windows_terminal.cs` (copied from [nikouu/TinyWorldle](https://github.com/nikouu/TinyWordle)) and then optmizied, e.g. using `LibraryImport()` instead of `DllImport()`)

Both files simply import native OS functionalities for their own terminal. That way, I don't have to rely on `System.Console`, which saves a lot of bytes.

## Mostly avoiding `System.String`

The `.mstat` file revealed that `System.String` wasted most amount of bytes. I mostly avoided using `System.String`. Instead, I used most of the time:
* `Span<T>`
* `ReadOnlySpan<T>`
* `char[]`
* `byte[]` (a byte is an unsigned int with a size of 2^8 bytes. As such, it already represents a character, when converted to ASCII)
* `stackalloc`

etc..

I have no definitive answer for everything but I did find out some reasons why `System.String` wasted many bytes. It's because various compiler/runtime messages (e.g. exceptions) and culture specific data are included into the binary. You can do settings in the `.csproj` file to mitigate this. Check out the project's `.csproj` file to see which settings I used. :)

Anyway, after mostly replacing `string`, the compiled binary size became much smaller.
