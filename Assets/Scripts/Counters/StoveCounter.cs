using System;
using System.Linq;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSo[] fryingRecipeSoArray;
    [SerializeField] private BurningRecipeSo[] burningRecipeSoArray;
    private State _state;
    private float _fryingTimer;
    private float _burningTimer;
    private FryingRecipeSo _fryingRecipeSo;
    private BurningRecipeSo _burningRecipeSo;


    private void Start()
    {
        _state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (_state)
            {
                case State.Idle:
                    break;
                case State.Frying:

                    _fryingTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
                    {
                        ProgressNormalized = _fryingTimer / _fryingRecipeSo.fryingTime
                    });
                    
                    if (_fryingTimer > _fryingRecipeSo.fryingTime)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(_fryingRecipeSo.output, this);
                        _burningRecipeSo = GetBurningRecipeFromInput(GetKitchenObject().KitchenObjectSo);
                        _burningTimer = 0f;
                        _state = State.Fried;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = _state
                        });
                        
                    }

                    break;
                case State.Fried:
                    _burningTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
                    {
                        ProgressNormalized = _burningTimer / _burningRecipeSo.burningTime
                    });
                    
                    if (_burningTimer > _burningRecipeSo.burningTime)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(_burningRecipeSo.output, this);
                        _state = State.Burned;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = _state
                        });
                    }

                    break;
                case State.Burned:
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
                    {
                        ProgressNormalized = 1f
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // There's nothing on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (HasFryingRecipeFromInput(player.GetKitchenObject().KitchenObjectSo))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    _fryingTimer = 0f;
                    _fryingRecipeSo = GetFryingRecipeFromInput(GetKitchenObject().KitchenObjectSo);
                    _state = State.Frying;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        State = _state
                    });
                }
            }
        }
        else
        {
            // There's a kitchen object on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (player.GetKitchenObject().TryGetPlate(out var plateKitchenObject))
                {
                    // Player is carrying a plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().KitchenObjectSo))
                    {
                        GetKitchenObject().DestroySelf();
                        _state = State.Idle;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = _state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
                        {
                            ProgressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
                _state = State.Idle;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = _state
                });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
                {
                    ProgressNormalized = 0f
                });
            }
        }
    }
    
    public bool IsFried()
    {
        return _state == State.Fried;
    }

    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO input)
    {
        return GetFryingRecipeFromInput(input)?.output;
    }

    private bool HasFryingRecipeFromInput(KitchenObjectSO input)
    {
        return GetFryingRecipeFromInput(input) != null;
    }

    private FryingRecipeSo GetFryingRecipeFromInput(KitchenObjectSO input)
    {
        return fryingRecipeSoArray.FirstOrDefault(fryingRecipeSo => fryingRecipeSo.input == input);
    }

    private BurningRecipeSo GetBurningRecipeFromInput(KitchenObjectSO input)
    {
        return burningRecipeSoArray.FirstOrDefault(burningRecipeSo => burningRecipeSo.input == input);
    }
}