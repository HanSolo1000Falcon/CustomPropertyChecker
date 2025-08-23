using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace CustomPropertyChecker;

public class PunCallbacks : MonoBehaviourPunCallbacks
{
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Hashtable properties = targetPlayer.CustomProperties;
        Plugin.PlayerProperties[GorillaParent.instance.vrrigs.FirstOrDefault(rig => rig.OwningNetPlayer.UserId == targetPlayer.UserId)] = properties;
    }
}