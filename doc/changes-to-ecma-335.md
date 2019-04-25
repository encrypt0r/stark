# Stark CLI (SKCLI)

This document is listing the changes to the ECMA-335 CLI required by stark

TODO:

- [ ] add changes to type specs
- [ ] add changes to type constraints

## In `II.23.1.10 Flags for methods [MethodAttributes]`

- `HasSecurity` is transformed to `IsReadOnly` `0x4000`: when `this` is `readonly`, only a method instance with `IsReadOnly` can be called. Only valid for method instance (doesn't work with static modifier)
- `RequireSecObject` is transformed to `RetainThis` `0x8000`: when `this` is `transient`, only a method instance without `RetainThis` can be called. Only valid for method instance (doesn't work with static modifier)

## In `II.23.1.11 Flags for methods [MethodImplAttributes]`

- Add FuncImplOptions.intrinsic attribute to identify "extern" methods that are actually implemented by the runtime as intrinsics

  In `Implementation info and interop`:  `Intrinsic = 0x2000`


