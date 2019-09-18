﻿namespace Artity.Services.Data.Artists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Artity.Data.Common.Repositories;
    using Artity.Data.Models;
    using Artity.Services.Messaging;
    using Mapping;

    using ServiceModels;

    using User;

    public class ArtistService : IArtistService
    {
        private readonly IRepository<Artist> artistContext;
        private readonly ISendGrid emailSender;
        private readonly IUserService userService;
        private readonly IRepository<Social> socialContext;

        public ArtistService(
            IRepository<Artist> artistContext,
            ISendGrid emailSender,
            IUserService userService,
            IRepository<Social> socialContext
            )
        {
            this.artistContext = artistContext;
            this.emailSender = emailSender;
            this.userService = userService;
            this.socialContext = socialContext;
        }

        public async Task<bool> AddSocial(string artistId, SocialServiceModel socialServiceModel)
        {
            var social = new Social() {
                 Facebook = socialServiceModel.Facebook,
                 Youtube = socialServiceModel.Youtube,
                 WebSite = socialServiceModel.WebSite,

            };
            var artist = this.Get(artistId);

            await this.socialContext.AddAsync(social);
            var w = await this.artistContext.SaveChangesAsync();
            artist.Social = social;

            this.artistContext.Update(artist);
            var result = await this.artistContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> ApprovedArtist(string id)
        {
            var approvedArtist = this.artistContext.All()
                .FirstOrDefault(a => a.Id == id);
            approvedArtist.IsApproved = true;

            this.artistContext.Update(approvedArtist);
            var result = await this.artistContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<SocialServiceModel> EditSocial(string artistId, SocialServiceModel socialServiceModel)
        {
            var social = socialServiceModel.MapTo<Social>();
            var artist = this.GetArtist(artistId).MapTo<Artist>();
            artist.Social = social;
            this.artistContext.Update(artist);
            var result = await this.artistContext.SaveChangesAsync();

            return socialServiceModel;
        }

        public IEnumerable<TViewModel> GetAllArtists<TViewModel>(bool isApproved)
        {
            return this.artistContext
                  .All()
                .Where(a => a.IsApproved == isApproved && a.IsDeleted != true)
                .OrderBy(a => a.CreatedOn)
                .To<TViewModel>().ToList();
        }

        public IEnumerable<TViewModel> GetAllArtists<TViewModel>()
        {
            return this.artistContext
                  .All()
                .Where(a => a.IsDeleted != true)
                .OrderBy(a => a.CreatedOn)
                .To<TViewModel>().ToList();
        }

        public IList<TViewModel> GetAllArtistsFiltretBy<TViewModel>(string filter)
        {
            throw new NotImplementedException();
        }

        public IList<TViewModel> GetAllArtiststFrom<TViewModel>(int category)
        {
            return this.artistContext
                 .All()
                 .Where(a => (int)a.Category.CategoryType == (int)category)
                 .OrderBy(a => a.CreatedOn)
                 .To<TViewModel>().ToList();
        }

        public IQueryable GetArtist(string id)
        {
            return this.artistContext
                  .All()
                  .Where(a => a.Id == id && a.IsDeleted == false);
        }

        public async Task<string> GetArtistIdByName(string name)
        {
            return this.artistContext
                  .All()?
                  .FirstOrDefault(a => a.Nikname == name).Id;
        }

        public async Task<SocialServiceModel> GetSocial(string artistId)
        {
            var social = this.artistContext
               .All()
               .FirstOrDefault(a => a.Id == artistId).Social;
            if (social == null)
            {
                  return new SocialServiceModel() { Facebook = social.Facebook, WebSite = social.WebSite, Youtube = social.Youtube };
            }
            return new SocialServiceModel() { Facebook = social.Facebook, WebSite = social.WebSite, Youtube = social.Youtube };
        }

        public async Task<bool> RefuseArtist(string id, string message)
        {
            string email = await this.userService.GetArtistEmail(id);

           await this.emailSender.SendEmailAsync(email,
                         "You ",
                         $"Sorry, the art team is delighted to have your access to a platform for playing your account," +
                         $" please contact < a href='support@artity.com.>support@artity.com.</a>."
                         +$"<h4>POSSIBLE CAUSES:</h4>"
                         + "1.Unlawful content"
                         + "2. Security issues"
                         + message
                         );

            this.artistContext.All()
                .FirstOrDefault(a => a.Id == id)
                .IsDeleted = true;

            var result = await this.artistContext.SaveChangesAsync();

            return result > 0;
        }

        Artist Get(string artistId)
        {
            return this.artistContext
                  .All()?
                  .FirstOrDefault(a => a.Id == artistId);
        }

        Artist IArtistService.Get(string artistId)
        {
            return this.artistContext
                   .All()?
                   .FirstOrDefault(a => a.Id == artistId);
        }
    }
}