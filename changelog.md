version 0.1.0 (yanked)
======================
- Initial release 
    
version 0.1.1 
=============

- Move PluralRules to netstandard 2.1
- Fix .ftl tests treated by Git as binary
    
version 0.1.2
=============

- Fix escaping 
- Add 3rd party licenses
- Nit - remove annoying warning
    
version 0.1.3
=============

- Fix row counting
- Fix error reports
- Make builds reproducible
    
version 0.1.4
=============

- Fix issue with number formatting using `CultureInfo.CurrentCulture` instead of `CultureInfo.InvariantCulture`. 
 Big thanks to @Mailaender

version 0.2.0
=============

- Linguini supports netstandard2.1 for use in Mono (this changes no API but introduces 
polyfills for netstandard2.1)
- Fix some stale dependencies
    
version 0.2.1
=============

- Fix issue with `System.Text.Json` leaking analyzers, causing issues on Mono
- Minor tweak of versions of `System.Text.Json` and enabling tests

version 0.2.2
=======

- Fix issue with `System.Text.Json` causing problems with Mono see https://github.com/mono/mono/issues/15833
> Chatted with @danroth27 and he mentioned that this could be due to mono lagging in netstandard2.1 support.
 I can imagine that if they were missing a typeforward in netstandard.dll for IAsyncDisposable this could happen.
 
 
version 0.3.0
========

- Extracted `Linguini.Syntax.Serializers` to a separate package
- Changed the way `InsertBehavior` works
- Add support for net461 @mtkennerly

version 0.3.1
========

- Fixed error in `Linguini.Bundle` that prevented term and scope arguments from coexisting. @adcdefg30

version 0.3.2
========

- Adds `LinguiniBundle.HasAttrMessage` method
- Obsoletes `LinguiniBundle.TryGetAttrMsg` for `LinguiniBundle.TryGetAttrMessage`
- Obsoletes `LinguiniBundle.TryGetMsg` for `LinguiniBundle.TryGetMessage`

version 0.4.0
========

- Changes default on `LinguiniBundle.SetIsolating` from `true` to `false`
- Adds method `GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args)` for ease of use.
- Removes `enum InsertBehavior` in favor of three separate functions (`TryAddFunction`, `AddFunctionOverriding`, `AddFunctionUnchecked`)
- Removes previously obsolete methods.

version 0.5.0
========

- Improves parsing performance by eliminating bounds check on `ReadOnlySpan<char>` for `char` @RoosterDragon
  Breaking changes:
  - ZeroCopyUtil
    - `TryReadCharSpan` replaced with `TryReadChar`
    - Methods `IsIdentifier`/`IsNumberStart`/`IsAsciiDigit`/`IsAsciiUppercase`/`IsAsciiHexdigit`/`IsAsciiAlphabetic`
      take char rather than `ReadOnlySpan<char>`
    - `EqualSpans` method removed
  - ZeroCopyReader
    - method signature `ReadOnlySpan<char> PeekCharSpan(int offset = 0)` changed to `char? PeekChar(int offset = 0)`
    - method `SeekEol` added.
    - methods `TryPeekChar`, `TryPeekCharAt`,`CurrentChar`, and  `IndexOfAnyChar` added.
  - ParserError
    - factory method for `ExpectedTokens` arguments changed
  - LinguiniParser
    - changed to use new ZeroCopyUtil internally.
  Non-breaking changes:
  - Fluent bundle private bundle method separated into `AddEntry` and `AddEntryOverriding`

version 0.6.0
========
- Fixes errors when reading line numbers due to interaction
  with ZeroCopy Parser (thanks to @PJB3005)
- Moves project to minimal dotnet version to 6