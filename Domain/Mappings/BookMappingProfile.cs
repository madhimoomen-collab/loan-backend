using AutoMapper;
using Domain.DTOs;
using Domain.Models;

namespace Domain.Mappings
{
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile()
        {
            CreateMap<Book, BookDto>();
            CreateMap<BookDto, Book>();
        }
    }
}