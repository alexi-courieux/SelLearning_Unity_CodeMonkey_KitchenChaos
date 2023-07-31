using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;
    
    [SerializeField] private CuttingRecipeSo[] cuttingRecipeSoArray;

    private int cuttingProgress;
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasCuttingRecipeFromInput(player.GetKitchenObject().KitchenObjectSo))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;
                    
                    var recipe = GetCuttingRecipeFromInput(GetKitchenObject().KitchenObjectSo);
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        ProgressNormalized = (float) cuttingProgress / recipe.cuttingProgressRequired
                    });
                }
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    ProgressNormalized = 0
                });
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if (!HasKitchenObject() || !HasCuttingRecipeFromInput(GetKitchenObject().KitchenObjectSo)) return;
        
        cuttingProgress++;
        var recipe = GetCuttingRecipeFromInput(GetKitchenObject().KitchenObjectSo);
        OnCut?.Invoke(this, EventArgs.Empty);
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            ProgressNormalized = (float) cuttingProgress / recipe.cuttingProgressRequired
        });
        
        if(recipe.cuttingProgressRequired > cuttingProgress) return;
        
        GetKitchenObject().DestroySelf();
        KitchenObject.SpawnKitchenObject(recipe.output, this);
    }

    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO input)
    {
        return GetCuttingRecipeFromInput(input)?.output;
    }
    
    private bool HasCuttingRecipeFromInput(KitchenObjectSO input)
    {
        return GetCuttingRecipeFromInput(input) != null;
    }
    
    private CuttingRecipeSo GetCuttingRecipeFromInput(KitchenObjectSO input)
    {
        return cuttingRecipeSoArray.FirstOrDefault(cuttingRecipeSo => cuttingRecipeSo.input == input);
    }
}