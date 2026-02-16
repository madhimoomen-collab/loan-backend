using AutoMapper;
using Domain.DTOs;
using Domain.Models;
#nullable disable warnings
namespace Domain.Mappings
{
    /// <summary>
    /// AutoMapper profile for Reservation entity and DTOs
    /// </summary>
    public class ReservationMappingProfile : Profile
    {
        public ReservationMappingProfile()
        {
            // Reservation -> ReservationDto (Full details)
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}"))
                .ForMember(dest => dest.ClientEmail,
                    opt => opt.MapFrom(src => src.Client.Email))
                .ForMember(dest => dest.BookTitle,
                    opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.BookISBN,
                    opt => opt.MapFrom(src => src.Book.ISBN))
                .ForMember(dest => dest.BookAuthor,
                    opt => opt.MapFrom(src => src.Book.Author))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.IsExpired,
                    opt => opt.MapFrom(src => src.IsExpired))
                .ForMember(dest => dest.DaysUntilExpiry,
                    opt => opt.MapFrom(src => src.DaysUntilExpiry));

            // Reservation -> ReservationListDto (Summary for lists)
            CreateMap<Reservation, ReservationListDto>()
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}"))
                .ForMember(dest => dest.BookTitle,
                    opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.IsExpired,
                    opt => opt.MapFrom(src => src.IsExpired));

            // CreateReservationDto -> Reservation
            CreateMap<CreateReservationDto, Reservation>()
                .ForMember(dest => dest.ReservationDate,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ExpiryDate,
                    opt => opt.MapFrom(src => DateTime.Now.AddDays(src.ReservationDurationDays)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => ReservationStatus.Active))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.PickupDate, opt => opt.Ignore())
                .ForMember(dest => dest.CancelledDate, opt => opt.Ignore());

            // UpdateReservationDto -> Reservation (for partial updates)
            CreateMap<UpdateReservationDto, Reservation>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}