using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Auditoria;
using Microservicio.Atracciones.DataManagement.Models.Auditoria;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class AuditoriaLogDataService : IAuditoriaLogDataService
    {
        private readonly IUnitOfWork _uow;

        public AuditoriaLogDataService(IUnitOfWork uow) => _uow = uow;

        public async Task RegistrarAsync(AuditoriaLogDataModel model)
        {
            var entity = AuditoriaLogDataMapper.ToNewEntity(model);
            await _uow.SaveChangesAsync();
        }
    }
}
