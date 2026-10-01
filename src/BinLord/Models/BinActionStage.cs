namespace BinLord.Models;

/// <summary>
/// Same-day reminder state for a bin that's due today: whether it still
/// needs putting out, is out awaiting collection, needs bringing back in,
/// or is all done.
/// </summary>
public enum BinActionStage
{
    NeedsPutOut,
    PutOut,
    NeedsBringIn,
    Done,
}
