using System;
using System.Threading.Tasks;
using PettingZoo.Core.Settings;

namespace PettingZoo.UI.Connection
{
    public interface IConnectionDialog
    {
        Task<ConnectionSettings?> Show();
    }
}
