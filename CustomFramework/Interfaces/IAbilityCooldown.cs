using LabApi.Features.Wrappers;
using System.Collections.Generic;

namespace CustomFramework.Interfaces {
    public interface IAbilityCooldown {
        float AbilityCooldown { get; }

        HashSet<Player> ActiveCooldowns { get; set; }
    }
}
