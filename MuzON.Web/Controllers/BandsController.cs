﻿using AutoMapper;
using MuzON.BLL.DTO;
using MuzON.BLL.Interfaces;
using MuzON.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MuzON.Web.Controllers
{
    public class BandsController : BaseController
    {
        public BandsController(IBandService artistServ, 
                                ICountryService countryServ, 
                                IArtistService bandServ)
            : base(artistServ, countryServ, bandServ) { }

        // GET: Bands
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetList()
        {
            var bandsDTO = bandService.GetBands();
            var bands = Mapper.Map<IEnumerable<BandIndexViewModel>>(bandsDTO);

            foreach (var item in bands)
            {
                item.CountryName = bandService.GetBandById(item.Id).Country.Name;
            }

            return Json(new { data = bands }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Details(Guid id)
        {
            var bandDTO = bandService.GetBandById(id);
            var band = Mapper.Map<BandDetailsViewModel>(bandDTO);
            return PartialView("_DetailsPartial", band);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            var model = new BandViewModel();
            model.CreatedDate = DateTime.Today;

            var countryDTOs = countryService.GetCountries();
            var countries = Mapper.Map<IEnumerable<CountryDTO>, IEnumerable<CountryViewModel>>(countryDTOs);
            ViewBag.CountryId = new SelectList(countries, "Id", "Name");

            var artistsDTO = artistService.GetArtists();
            var artists = Mapper.Map<IEnumerable<ArtistViewModel>>(artistsDTO);
            ViewBag.Artists = new MultiSelectList(artists, "Id", "FullName");
            
            // viewbag for post
            ViewBag.Action = "create";
            return PartialView("_CreateAndEditPartial", model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BandViewModel bandViewModel, Guid[] SelectedArtists, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                var bandDTO = Mapper.Map<BandDTO>(bandViewModel);
                bandDTO.Id = Guid.NewGuid();

                bandDTO.Image = util.SetImage(uploadImage, bandDTO.Image);

                bandDTO.Country = countryService.GetCountryById(bandViewModel.CountryId);
                bandService.AddBand(bandDTO, SelectedArtists);
                return RedirectToAction("Index");
            }
            var artistsDTO = artistService.GetArtists();
            var artists = Mapper.Map<IEnumerable<ArtistViewModel>>(artistsDTO);
            ViewBag.Artists = new MultiSelectList(artists, "Id", "FullName");

            var countryDTOs = countryService.GetCountries();
            var countries = Mapper.Map<IEnumerable<CountryDTO>, IEnumerable<CountryViewModel>>(countryDTOs);
            ViewBag.CountryId = new SelectList(countries, "Id", "Name");
            return PartialView("_CreateAndEditPartial", bandViewModel);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Edit(Guid id)
        {
            var band = bandService.GetBandById(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            TempData["Artists"] = band.Artists;
            BandViewModel bandViewModel = Mapper.Map<BandViewModel>(band);
            

            var countryDTOs = countryService.GetCountries();
            var countries = Mapper.Map<IEnumerable<CountryViewModel>>(countryDTOs);
            ViewBag.CountryId = new SelectList(countries, "Id", "Name", bandViewModel.CountryId);

            var artistsDTO = from a in artistService.GetArtists()
                             select a;
            var artists = Mapper.Map<IEnumerable<ArtistViewModel>>(artistsDTO);
            if (band.Artists != null)
            {
                bandViewModel.SelectedArtists = new List<Guid>();
                foreach (var item in band.Artists)
                {
                    bandViewModel.SelectedArtists.Add(item.Id);
                }
            }
            ViewBag.Artists = new MultiSelectList(artists, "Id", "FullName");

            // viewbag for post
            ViewBag.Action = "edit";
            return PartialView("_CreateAndEditPartial", bandViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BandViewModel bandViewModel, Guid[] SelectedArtists, HttpPostedFileBase uploadImage, string image)
        {

            if (ModelState.IsValid)
            {
                var bandDTO = Mapper.Map<BandViewModel, BandDTO>(bandViewModel);
                bandDTO.Artists = TempData["Artists"] as List<ArtistDTO>;
                bandDTO.Image = util.SetImage(uploadImage, bandDTO.Image, image);
                bandDTO.Country = countryService.GetCountryById(bandViewModel.CountryId);
                bandService.UpdateBand(bandDTO, SelectedArtists);
                return RedirectToAction("Index");
            }
            var countryDTOs = countryService.GetCountries();
            var countries = Mapper.Map<IEnumerable<CountryDTO>, IEnumerable<CountryViewModel>>(countryDTOs);
            ViewBag.CountryId = new SelectList(countries, "Id", "Name", bandViewModel.CountryId);
            return PartialView("_CreateAndEditPartial", bandViewModel);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Delete(Guid id)
        {
            var band = bandService.GetBandById(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            BandViewModel bandViewModel = Mapper.Map<BandViewModel>(band);
            return PartialView("_DeletePartial", bandViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var bandDTO = bandService.GetBandById(id);
            bandService.DeleteBand(bandDTO);
            return RedirectToAction("Index");
        }
    }
}