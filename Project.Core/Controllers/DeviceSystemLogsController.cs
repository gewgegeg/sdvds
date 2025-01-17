﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monit.Project.Core.Models;
using Monit.Project.Core.Services.DeviceStateService;
using Monit.Project.Core.Services.DeviceSystemLogsService;

namespace Monit.Project.Core.Controllers
{
    [Route("api/device/system/logs")]
    [ApiController]
    public class DeviceSystemLogsController : ControllerBase
    {
        private readonly IDeviceSystemLogsService _stateSerivce;
        public DeviceSystemLogsController(IDeviceSystemLogsService service)
        {
            _stateSerivce = service;
        }

        [HttpPut("set")]
        public async Task<ActionResult<Guid>> SetLogs([FromBody] DeviceSystemLogs systemLogs)
        {
            await _stateSerivce.SetLogs(systemLogs);

            return Ok();
        }

        [HttpGet("get")]
        public async Task<ActionResult<DeviceSystemLogs>> GetLogs(Guid id)
        {
            var device = await _stateSerivce.GetLogsOrNull(id);

            if (device is null)
                return NotFound();

            return Ok(device);
        }
    }
}
