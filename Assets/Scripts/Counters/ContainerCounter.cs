using System;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabObject;

    [SerializeField] protected KitchenObjectSO kitchenObjectSo;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject()) return;

        // Spawn a new kitchen object
       KitchenObject.SpawnKitchenObject(kitchenObjectSo, player);
        OnPlayerGrabObject?.Invoke(this, EventArgs.Empty);
    }
}