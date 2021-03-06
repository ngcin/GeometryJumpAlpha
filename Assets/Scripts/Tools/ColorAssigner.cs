using UnityEngine;

[ExecuteInEditMode]
public abstract class ColorAssigner : MonoBehaviour
{
    [SerializeField] private ColorReference color;
    [SerializeField] [Range(-360f, 360f)] public float hue;
    [SerializeField] [Range(-1f, 1f)] public float sat;
    [SerializeField] [Range(-1f, 1f)] public float val;
    [SerializeField] [Range(-1f, 1f)] public float alpha;

    private Color savedColor;
    private int start = 0;

    public ColorReference ColorReference
    {
        get => color;
        set
        {
            if (color == value)
                return;

            ColorReference oldValue = color;

            if (oldValue != null && value == null)
                UnsubscribeColorChange();

            color = value;

            if (oldValue == null && value != null)
                SubscribeColorChange();
        }
    }

    protected abstract void AssignColor(Color color, float hue, float sat, float val, float alpha);

    private void SubscribeColorChange() => color.Changed += OnColorChanged;

    private void UnsubscribeColorChange() => color.Changed -= OnColorChanged;

    private void OnColorChanged() => AssignColor(color, hue, sat, val, alpha);

    
    private void Start()
    {
        start = 1;
        if (color != null)
        {
            UnsubscribeColorChange();
            SubscribeColorChange();
            AssignColor(color, hue, sat, val, alpha);
        }
        if(color.flag)
        {
            //Debug.Log("FLAG");
            color.flag = false;
            color.channelcolor = color.refer.channelcolor;
            AssignColor(color.refer.channelcolor, hue, sat, val, alpha);
            SubscribeColorChange();
        }

        savedColor.r = color.r;
        savedColor.g = color.g;
        savedColor.b = color.b;
        savedColor.a = color.a;
    }

    private void OnDestroy()
    {
        if (color != null)
        {
            UnsubscribeColorChange();
        }
        if (start == 1)
        {
            color.Set(savedColor);
            SubscribeColorChange(); // added
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(color.refer != null)
        {
            color.flag = true;
        }
        if (color != null)
        {
            UnsubscribeColorChange();
            SubscribeColorChange();
            OnColorChanged();
            
            // added
            savedColor.r = color.r;
            savedColor.g = color.g;
            savedColor.b = color.b;
            savedColor.a = color.a;//*/
        }
    }
#endif

}