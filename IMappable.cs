namespace GenericMapper;

public interface IMappable<in TDto, out TModel> where TModel : IMappable<TDto, TModel>
{
    static abstract TModel Map(TDto dto);
}

public record OrderModel : IMappable<OrderDto, OrderModel>
{
    public int Id { get; set; }
    
    // static factory method required by the compiler
    public static OrderModel Map(OrderDto dto) => new()
    {
        Id = dto.Id
        // map manually here
    };
}

public record OrderDto
{
    public int Id { get; set; }
}

// centralized null handling, available to every IMappable<, > implementer
public static class MappableExtensions
{
    extension<TModel, TDto>(TModel) where TModel : class, IMappable<TDto, TModel>
    {
        public static TModel? FromDto(TDto dto)
        {
            return MapOrNull<TModel, TDto>(dto);
        }

        public static List<TModel?> FromDtos(IEnumerable<TDto> dtos)
        {
            return dtos?.Select(MapOrNull<TModel, TDto>).ToList() ?? [];
        }
        
        private static TModel? MapOrNull(TDto dto)
        {
            return dto is null ? null : TModel.Map(dto);
        }
    }
}