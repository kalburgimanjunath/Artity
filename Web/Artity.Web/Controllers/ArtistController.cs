﻿namespace Artity.Web.Controllers
{
    using Artity.Services;
    using Artity.Services.File;
    using Artity.Services.Messaging;
    using Artity.Web.ViewModels.Artist;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class ArtistController : BaseController
    {
        private readonly IArtistService artistService;
        private readonly ISendGrid emailSender;
        private readonly ICategoryService categoryService;

        public ArtistController(IArtistService artistService,ICategoryService categoryService)
        {
            this.artistService = artistService;
            this.emailSender = emailSender;
            this.categoryService = categoryService;
        }

     
        public async Task<IActionResult> All()
        {
            var artists = this.artistService
                .GetAllArtists<ArtistAllViewModel>();

            var categories = this.categoryService
                .GetAllCategories();

            var artitView = new ArtistViewModel()
            {
                ArtistAlls = artists,
                Categories = categories,
            };

            return this.View(artitView);
        }

        public async Task<IActionResult> Dashboard()
        {
            return this.View();
        }

    }
}