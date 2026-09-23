# GenericMapper

A proof of concept for replacing AutoMapper with hand-written mappings. The compiler checks every mapping, and one shared helper layer handles nulls. There's no reflection, no profiles and no DI registration.

## How it works

1. A model implements `IMappable<TDto, TModel>`. The compiler then requires it to provide a `static Map(TDto)` method, where you write the mapping by hand.
2. A single extension block in `MappableExtensions` gives every implementer null-safe `FromDto` / `FromDtos` methods automatically.

```csharp
public record OrderModel : IMappable<OrderDto, OrderModel>
{
    public int Id { get; set; }

    public static OrderModel Map(OrderDto dto) => new() { Id = dto.Id };
}

var model  = OrderModel.FromDto(dto);   // null dto  -> null
var models = OrderModel.FromDtos(dtos); // null list -> empty list
var raw    = OrderModel.Map(dto);       // no null guard, throws on null
```

## Why this needs .NET 10

| Feature | Since | What it enables here |
|---|---|---|
| [Static abstract interface members](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface#static-abstract-and-virtual-members) | C# 11 / .NET 7 | Generic code can call `TModel.Map(dto)` directly, with no reflection and no `new()` + instance-method workaround. |
| [Static extension members](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#extension-members) (`extension<T>(T)` blocks) | C# 14 / .NET 10 | `OrderModel.FromDto(dto)` reads like a member of the model, even though it's defined once for every `IMappable` type. |

Before C# 14, the shared helper had to be a plain generic method, and every call needed explicit type arguments (`Mapper.FromDto<OrderModel, OrderDto>(dto)`). Without them the build fails with `CS0411`. To see the version dependency yourself, run `dotnet build -p:LangVersion=13`, which fails with `CS9260: Feature 'extensions' is not available in C# 13.0`.

## Replacing `ProjectTo`

`Map` is compiled code, so EF can't translate it into SQL. For `IQueryable`, define the mapping as an expression tree and pass it to a plain `Select`:

```csharp
public record OrderModel
{
    public static Expression<Func<OrderEntity, OrderModel>> Projection { get; } = e => new() { Id = e.Id };
}

db.Orders.Where(o => o.Id > 1).Select(OrderModel.Projection);
// SELECT "o"."Id" FROM "Orders" AS "o" WHERE "o"."Id" > 1   (EF Core 10, SQLite)
```

To avoid writing the mapping twice, `Map` can call a cached `Projection.Compile()` delegate.
