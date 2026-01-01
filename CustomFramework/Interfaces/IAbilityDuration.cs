using LabApi.Features.Wrappers;
using System.Collections.Generic;

namespace CustomFramework.Interfaces {
    public interface IAbilityDuration {
        float AbilityDuration { get; }

        HashSet<Player> ActiveAbilities { get; set; }
    }
}
