﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Monit.Project.Core.Models;
using Monit.Project.DAL.Entities;
using Monit.Project.DAL.Enums;

namespace Monit.Project.Core.Services.DeviceStateService
{
    public class DeviceStateService : IDeviceStateService
    {
        private readonly Dictionary<Guid, DeviceState> _connectedDevicesState = new Dictionary<Guid, DeviceState>();
        private Timer _stateUpdater;

        public DeviceStateService(IServiceScopeFactory scopeFactory)
        {
            var rng = new Random();
            IDataService<DeviceStaticInfo> deviceInfoService;
            using (var scope = scopeFactory.CreateScope())
            {
                deviceInfoService = scope.ServiceProvider.GetRequiredService<IDataService<DeviceStaticInfo>>();
                foreach (var device in deviceInfoService.Get(x => true).Result)
                {
                    _connectedDevicesState[device.Id] = new DeviceState()
                    {
                        SourceDeviceId = device.Id,
                        LastAnswerTime = 0.0,
                        RunTimeS = 0,
                        Status = (DeviceStatus)rng.Next(0, 4)
                    };
                }
            }

            var timer = new Timer(2000);
            timer.Elapsed += (source, e) =>
            {
                foreach (var deviceState in _connectedDevicesState.Where(device => device.Value.Status == DeviceStatus.Enabled))
                {
                    deviceState.Value.RunTimeS += 2;
                }

                foreach (var deviceState in _connectedDevicesState.Where(device => device.Value.Status != DeviceStatus.Enabled))
                {
                    deviceState.Value.LastAnswerTime += 2;
                }
            };
            timer.Start();
            _stateUpdater = timer;
            Console.WriteLine("Device state emulation started...");
        }

        public Task<DeviceState> GetStateOrNull(Guid id)
        {
            #nullable disable
            return Task.Run(() =>
            {
                _connectedDevicesState.TryGetValue(id, out var res);
                return res;
            });
            #nullable restore
        }

        public Task<Guid> SetState(DeviceState entity)
        {
            return Task.Run(() =>
            {
                _connectedDevicesState[entity.SourceDeviceId] = entity;
                return entity.SourceDeviceId;
            });
        }
    }
}
