using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace CustomPropertyChecker;

public class PunCallbacks : MonoBehaviourPunCallbacks
{
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Hashtable properties = targetPlayer.CustomProperties;
        Plugin.PlayerProperties[
                GorillaParent.instance.vrrigs.FirstOrDefault(rig => rig.OwningNetPlayer.UserId ==
                                                                    targetPlayer.UserId)] = properties;
    }
}