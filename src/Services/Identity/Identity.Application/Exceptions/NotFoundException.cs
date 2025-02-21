namespace Identity.Application.Exceptions
{
    public class NotFoundException(string entity, object key)
       : Exception($"{entity} with id {key} not found");
}