using AutoMapper;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class HospitalService : IHospitalService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public HospitalService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── POST /api/admin/hospitals ─────────────────────────────────────────
        public async Task<HospitalDto> CreateAsync(CreateHospitalDto dto)
        {
            // Validate unique name
            var nameSpec = new HospitalByNameSpecification(dto.Name);
            var existing = await _uow.Hospitals.GetEntityWithSpecAsync(nameSpec);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A hospital named '{dto.Name}' already exists.");

            // Validate unique email (only when provided)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailSpec = new HospitalByEmailSpecification(dto.Email);
                var emailDup = await _uow.Hospitals.GetEntityWithSpecAsync(emailSpec);
                if (emailDup is not null)
                    throw new InvalidOperationException(
                        $"A hospital with email '{dto.Email}' already exists.");
            }

            var hospital = new Hospital
            {
                Name = dto.Name.Trim(),
                Email = dto.Email?.Trim().ToLower(),
                Phone = dto.PhoneNumber?.Trim(),
                Address = dto.Address?.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.Hospitals.AddAsync(hospital);
            await _uow.SaveChangesAsync();

            var result = _mapper.Map<HospitalDto>(hospital);
            result.Message = "Hospital created successfully";
            return result;
        }

        // ── GET /api/admin/hospitals ──────────────────────────────────────────
        public async Task<HospitalsDashboardDto> GetAllAsync()
        {
            // COUNT — lightweight single query
            var countSpec = new AllHospitalsCountSpecification();
            var totalCount = await _uow.Hospitals.CountAsync(countSpec);

            // DATA — all hospitals ordered by Name ASC
            var dataSpec = new AllHospitalsSpecification();
            var hospitals = await _uow.Hospitals.GetAllWithSpecAsync(dataSpec);

            return new HospitalsDashboardDto
            {
                Statistics = new HospitalStatisticsDto { TotalHospitals = totalCount },
                Hospitals = _mapper.Map<IEnumerable<HospitalListItemDto>>(hospitals),
            };
        }

        // ── PUT /api/admin/hospitals/{id} ─────────────────────────────────────
        public async Task<HospitalDto> UpdateAsync(int id, UpdateHospitalDto dto)
        {
            // Validate hospital exists
            var spec = new HospitalByIdSpecification(id);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {id} was not found.");

            // Validate unique name (exclude self)
            var nameSpec = new HospitalByNameSpecification(dto.Name, excludeId: id);
            var nameDup = await _uow.Hospitals.GetEntityWithSpecAsync(nameSpec);
            if (nameDup is not null)
                throw new InvalidOperationException(
                    $"A hospital named '{dto.Name}' already exists.");

            // Validate unique email (exclude self, only when provided)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailSpec = new HospitalByEmailSpecification(dto.Email, excludeId: id);
                var emailDup = await _uow.Hospitals.GetEntityWithSpecAsync(emailSpec);
                if (emailDup is not null)
                    throw new InvalidOperationException(
                        $"A hospital with email '{dto.Email}' already exists.");
            }

            // Apply changes
            hospital.Name = dto.Name.Trim();
            hospital.Email = dto.Email?.Trim().ToLower();
            hospital.Phone = dto.PhoneNumber?.Trim();
            hospital.Address = dto.Address?.Trim();
            hospital.UpdatedAt = DateTime.UtcNow;

            _uow.Hospitals.Update(hospital);
            await _uow.SaveChangesAsync();

            var result = _mapper.Map<HospitalDto>(hospital);
            result.Message = "Hospital updated successfully";
            return result;
        }

        // ── DELETE /api/admin/hospitals/{id} ──────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var spec = new HospitalByIdSpecification(id);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {id} was not found.");

            _uow.Hospitals.Delete(hospital);
            await _uow.SaveChangesAsync();
        }

        // ── GET /api/hospitals/dropdown ───────────────────────────────────────
        public async Task<IEnumerable<HospitalDropdownItemDto>> GetDropdownAsync()
        {
            var spec = new HospitalsDropdownSpecification();
            var hospitals = await _uow.Hospitals.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<HospitalDropdownItemDto>>(hospitals);
        }
    }
}
