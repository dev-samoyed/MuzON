﻿using AutoMapper;
using MuzON.BLL.DTO;
using MuzON.BLL.Interfaces;
using MuzON.Domain.Entities;
using MuzON.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuzON.BLL.Services
{
    public class BandService : IBandService
    {
        private IUnitOfWork _unitOfWork;

        public BandService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public void AddBand(BandDTO bandDTO, Guid[] selectedArtists)
        {
            var band = Mapper.Map<Band>(bandDTO);
            if (selectedArtists != null)
            {
                band.Artists = new List<Artist>();

                foreach (var c in _unitOfWork.Artists.SearchFor(co => selectedArtists.Contains(co.Id)))
                {
                    band.Artists.Add(c);
                }
            }
            band.CountryId = bandDTO.Country.Id;
            band.Country = _unitOfWork.Countries.Get(band.CountryId);
            _unitOfWork.Bands.Create(band);
            _unitOfWork.Save();
        }

        public void DeleteBand(BandDTO bandDTO)
        {
            Band band = Mapper.Map<Band>(bandDTO);
            _unitOfWork.Bands.Delete(band.Id);
            _unitOfWork.Save();
        }

        public BandDTO GetBandById(Guid id)
        {
            var band = _unitOfWork.Bands.Get(id);
            return Mapper.Map<BandDTO>(band);
        }

        public IEnumerable<BandDTO> GetBands()
        {
            var bandsDTO = _unitOfWork.Bands.GetAll().ToList();
            return Mapper.Map<IEnumerable<BandDTO>>(bandsDTO);
        }

        public void UpdateBand(BandDTO bandDTO, Guid[] selectedArtists)
        {
            var band = Mapper.Map<Band>(bandDTO);
            if (selectedArtists != null)
            {
                if (band.Artists == null)
                    band.Artists = new List<Artist>();
                band.Artists.Clear();
                foreach (var c in _unitOfWork.Artists
                            .SearchFor(co => selectedArtists.Contains(co.Id)))
                {
                    band.Artists.Add(c);
                }
            }
            if (band.CountryId != bandDTO.Country.Id)
            {
                band.CountryId = bandDTO.Country.Id;
                band.Country = _unitOfWork.Countries.Get(band.CountryId);
            }
            _unitOfWork.Bands.Update(band);
            _unitOfWork.Save();
        }
    }
}
