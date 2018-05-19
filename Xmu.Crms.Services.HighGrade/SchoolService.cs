using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.HighGrade
{
    public class SchoolService : Orleans.Grain, ISchoolService
    {
        private readonly CrmsContext _db;

        public SchoolService(CrmsContext db) => _db = db;

        public async Task<School> GetSchoolBySchoolIdAsync(long schoolId)
        {
            var school = await _db.School.SingleOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            return school;
        }

        public async Task<long> InsertSchoolAsync(School school)
        {
            var s = new School
            {
                Id = school.Id,
                Name = school.Name,
                Province = school.Province,
                City = school.City
            };

            await _db.School.AddAsync(s);
            await _db.SaveChangesAsync();
            return s.Id;
        }

        public async Task<IList<string>> ListCityAsync(string province)
        {
            var cities = await _db.School.Where(s => s.Province == province).Select(s => s.City).Distinct().ToListAsync();

            if (cities == null)
            {
                throw new Exception();
            }

            return cities;
        }

        public async Task<IList<string>> ListProvinceAsync()
        {
            var provinces = await _db.School.Select(s => s.Province).Distinct().ToListAsync();

            if (provinces == null)
            {
                throw new Exception();
            }

            return provinces;
        }

        public async Task<IList<School>> ListSchoolByCityAsync(string city)
        {
            var schools = await _db.School.Where(s => s.City == city).ToListAsync();

            if (schools == null)
            {
                throw new Exception();
            }

            return schools;
        }
    }
}