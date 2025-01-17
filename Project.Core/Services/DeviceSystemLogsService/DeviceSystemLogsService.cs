﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Monit.Project.Core.Models;
using Monit.Project.Core.Services.DeviceStateService;
using Monit.Project.DAL.Entities;

namespace Monit.Project.Core.Services.DeviceSystemLogsService
{
    public class DeviceSystemLogsService : IDeviceSystemLogsService
    {
        private readonly Dictionary<Guid, DeviceSystemLogs> _connectedDevicesLogs = new();
        private readonly Timer _stateUpdater;

        // Заглушка - эмитатор работы девайсов
        public DeviceSystemLogsService(IServiceScopeFactory scopeFactory)
        {
            var rng = new Random();
            IDataService<DeviceStaticInfo> deviceInfoService;
            using (var scope = scopeFactory.CreateScope())
            {
                deviceInfoService = scope.ServiceProvider.GetRequiredService<IDataService<DeviceStaticInfo>>();
                foreach (var device in deviceInfoService.Get(device => true).Result)
                {
                    _connectedDevicesLogs[device.Id] = new DeviceSystemLogs()
                    {
                        CpuPerformanceGraph = new Queue<double>(),
                        SourceDeviceId = device.Id,
                        TerminalLog = new List<string>() { $"> Emulated console. Device: {device.Name}" }
                    };
                }
            }

            var timer = new Timer(2000);
            timer.Elapsed += (source, e) =>
            {
                foreach (var deviceState in _connectedDevicesLogs)
                {
                    if (deviceState.Value.CpuPerformanceGraph?.Count > 10)
                        deviceState.Value.CpuPerformanceGraph?.Dequeue();
                    deviceState.Value.CpuPerformanceGraph?.Enqueue(rng.Next(100) + rng.NextDouble());
                }
            };
            timer.Start();
            _stateUpdater = timer;
            Console.WriteLine("Device system logs emulation started...");
        }

        public Task<DeviceSystemLogs> GetLogsOrNull(Guid id)
        {
            return Task.Run(() =>
            {
                _connectedDevicesLogs.TryGetValue(id, out var device);
                return device!;
            });
        }

        public Task<Guid> SetLogs(DeviceSystemLogs entity)
        {
            return Task.Run(() =>
            {
                _connectedDevicesLogs[entity.SourceDeviceId] = entity;
                return entity.SourceDeviceId;
            });
        }

        public Task AddTerminalLog(Guid id, string terminalLog)
        {
            return Task.Run(() =>
            {
                if (_connectedDevicesLogs.ContainsKey(id))
                    _connectedDevicesLogs[id].TerminalLog?.Add(terminalLog);
            });
        }
    }
}
