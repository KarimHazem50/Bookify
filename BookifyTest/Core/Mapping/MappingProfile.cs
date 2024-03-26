﻿using Bookify.Domain.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Category, CategoryFormViewModel>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Books
            CreateMap<BookFormViewModel, Book>().ReverseMap();

            CreateMap<Book, BookViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Authors!.Name))
                .ForMember(dest => dest.CategoriesNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));

            CreateMap<Book, BookRowViewModel>()
               .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Authors!.Name))
               .ForMember(dest => dest.CategoriesNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));

            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                .ForMember(dest => dest.IsAvailableForRentalForMainBook, opt => opt.MapFrom(src => src.Book!.IsAvailableForRental))
                .ForMember(dest => dest.ImageName, opt => opt.MapFrom(src => src.Book!.ImageName));

            CreateMap<BookCopy, BookCopyFormViewModel>()
                .ForMember(dest => dest.ShowRentalInput, opt => opt.MapFrom(src => src.Book!.IsAvailableForRental));

            CreateMap<Book, BookSearchResultViewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Authors!.Name));

            CreateMap<BookDto, BookViewModel>();

            // Users
            CreateMap<ApplicationUser, UsersViewModel>();

            CreateMap<UserFormViewModel, CreateUserDto>();

            CreateMap<UserFormViewModel, ApplicationUser>()
                 .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                 .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                 .ReverseMap();

            // Subscribers
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<Area, SelectListItem>()
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<SubscriberFormViewModel, Subscriber>().ReverseMap();


            CreateMap<Subscriber, SubscriberSearchResultViewModel>();

            CreateMap<Subscriber, SubscriberDetailsViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.GovernorateName, opt => opt.MapFrom(src => src.Governorate!.Name))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area!.Name));

            // Subscription
            CreateMap<Subscription, SubscriptionViewModel>();

            // Rentals
            CreateMap<RentalCopy, RentalCopyViewModel>();
            CreateMap<Rental, RentalViewModel>();
            CreateMap<RentalCopy, CopyHistoryViewModel>()
               .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Rental!.Subscriber!.MobileNumber))
               .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => $"{src.Rental!.Subscriber!.FirstName} {src.Rental!.Subscriber!.LastName}"));

            //General
            CreateMap<KeyValuePairDto, ChartItemViewModel>()
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Key));
        }
    }
}
