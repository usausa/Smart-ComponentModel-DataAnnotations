# Diagnostics

| ID | Severity | Description | How to fix |
|---|---|---|---|
| SCD0001 | ❌ Error | Member referenced by the attribute is not found, or has no getter accessible from the assembly | Pass the name of an existing property with a `public` or `internal` getter (use `nameof`) |
| SCD0002 | ❌ Error | Condition member is not `bool` or `bool?` | Use a `bool` or `bool?` property as the condition |
| SCD0003 | ❌ Error | `[CompareTo]` members are not the same type, or the type does not implement `IComparable<T>` or `IComparable` | Compare members of the same comparable type |
| SCD0004 | ❌ Error | `[DuplicateKey]` member is not a collection, or is a `string` | Apply the attribute to a collection property |
| SCD0005 | ❌ Error | Type is generic, `file`-local, or not accessible from the assembly (for example a `private` nested type) | Make the type non-generic and `internal` or `public` |
| SCD0006 | ⚠️ Warning | Member is a non-nullable value type, so it always has a value | Use a nullable type, or remove the attribute |
| SCD0007 | ❌ Error | Attribute argument is invalid (empty member list, empty name, or negative count) | Pass valid arguments |
| SCD0008 | ❌ Error | Attribute that reads other members is applied to a method parameter, or to a primary constructor parameter of a non-record type | Apply the attribute to a property or a record primary constructor parameter |
