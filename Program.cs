using GenericMapper;

OrderDto[] dtos =
[
    new()
    {
        Id = 1
    },
    new()
    {
        Id = 2
    },
    new()
    {
        Id = 3
    },
];
var model = OrderModel.FromDto(dtos[0]); // null-safe
var models = OrderModel.FromDtos(dtos); // null-safe
var mayThrow = OrderModel.Map(dtos[0]); // this throws on null
Console.WriteLine(string.Join(", ", model, models, mayThrow));