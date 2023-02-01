using static UnityEngine.Mathf;

namespace RegionKit.Modules.Particles.V1;
#region spawners
#endregion

public class ParticleVisualCustomizer : ManagedData, IParticleVisualProvider
{
	[ColorField("sColBase", 1f, 1f, 1f, 1f, DisplayName: "Sprite color base")]
	public Color spriteColor;
	[ColorField("sColFluke", 0f, 0f, 0f, 0f, DisplayName: "Sprite color fluke")]
	public Color spriteColorFluke;
	[ColorField("lColBase", 1f, 1f, 1f, 1f, DisplayName: "Light color base")]
	public Color lightColor;
	[ColorField("lColFluke", 0f, 0f, 0f, 0f, DisplayName: "Light color fluke")]
	public Color lightColorFluke;
	[BooleanField("flat", false, displayName: "Flat light")]
	public bool flatLight;
	[FloatField("lrminBase", 0f, 400f, 20f, displayName: "Light radius min")]
	public float lightRadMin = 20f;
	[FloatField("lrminFluke", 0f, 400f, 0f, displayName: "Lightradmin fluke")]
	public float lightRadMinFluke = 0f;
	[FloatField("lrmaxBase", 0f, 400f, 30f, displayName: "Light radius max")]
	public float lightRadMax = 30f;
	[FloatField("lrmaxFluke", 0f, 400f, 0f, displayName: "Lightradmax fluke")]
	public float lightRadMaxFluke = 0f;
	[FloatField("lIntBase", 0f, 1f, 1f, displayName: "Light intensity")]
	public float LightIntensity = 1f;
	[FloatField("lIntFluke", 0f, 1f, 0f, displayName: "Light intensity fluke")]
	public float LightIntensityFluke = 0f;
	[StringField("eName", "SkyDandelion", displayName: "Atlas element")]
	public string elmName = "SkyDandelion";
	[StringField("shader", "Basic", displayName: "Shader")]
	public string shader = "Basic";
	[Vector2Field("p2", 40f, 0f, Vector2Field.VectorReprType.circle)]
	public Vector2 p2;
	[EnumField<ContainerCodes>("cc", ContainerCodes.Foreground)]
	public ContainerCodes containerCode;
	[FloatField("z_scalemin", 0.1f, 2f, 1f, 0.05f, ManagedFieldWithPanel.ControlType.slider, displayName: "scale min")]
	public float scalemin = 1f;
	[FloatField("z_scalemax", 0.1f, 2f, 1f, 0.05f, ManagedFieldWithPanel.ControlType.slider, displayName: "scale max")]
	public float scalemax = 1f;

	public ParticleVisualCustomizer(PlacedObject owner) : base(owner, null)
	{

	}

	public Vector2 P2 => p2;

	public PlacedObject Owner => owner;

	public ParticleVisualState DataForNew()
	{
		var res = new ParticleVisualState(
			elmName,
			shader,
			containerCode,
			spriteColor.Deviation(spriteColorFluke),
			lightColor.Deviation(lightColorFluke),
			ClampedFloatDeviation(LightIntensity, LightIntensityFluke, minRes: 0f),
			ClampedFloatDeviation(lightRadMax, lightRadMaxFluke, minRes: 0f),
			ClampedFloatDeviation(lightRadMin, lightRadMinFluke, minRes: 0f),
			0f,
			flatLight,
			Lerp(scalemin, scalemax, UnityEngine.Random.value))
		{
			//sCol = spriteColor.Deviation(spriteColorFluke),
			//lCol = lightColor.Deviation(lightColorFluke),
			//lRadMin = ClampedFloatDeviation(lightRadMin, lightRadMinFluke, minRes: 0f),
			//lRadMax = ClampedFloatDeviation(lightRadMax, lightRadMaxFluke, minRes: 0f),
			//lInt = ClampedFloatDeviation(LightIntensity, LightIntensityFluke, minRes: 0f),
			//aElm = elmName,
			//shader = shader,
			//container = cc,
			//flat = flatLight,
			//scale = Lerp(scalemin, scalemax, UnityEngine.Random.value),
		};
		res.spriteColor.ClampToNormal();
		res.lightColor.ClampToNormal();
		return res;
	}
}
