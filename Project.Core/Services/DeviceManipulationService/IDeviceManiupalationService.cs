﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monit.Project.Core.Services.DeviceManipulationService
{
    public interface IDeviceManiupalationService
    {
        public Task<List<string>> Execute(Guid id, string command);
    }
}
