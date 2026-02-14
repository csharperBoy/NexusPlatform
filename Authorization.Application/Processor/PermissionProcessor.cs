using Core.Application.Abstractions.Authorization;
using Core.Shared.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Processor
{
    public class PermissionProcessor : IPermissionProcessor
    {
        /*public Task<List<PermissionDto>> CalculateFinalPermission(List<PermissionDto> permissions)
		{
			try
			{
                List<PermissionDto> result = new List<PermissionDto>();
                var permissionGroupByResource = permissions.GroupBy(x => x.ResourceId).ToList();
                foreach (var permission in permissionGroupByResource)
                {
                    PermissionDto per = new PermissionDto();

                    per.ResourceId = // resourceId;
                    per.Action
                }
            }
			catch (Exception ex)
			{

				throw;
			}
        }*/
    }
}
