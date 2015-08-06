using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class skydomeScript2 : MonoBehaviour {
	// reference to objects
	public Light sunLight;
    public Camera cam;
    // private reference variables
	GameObject sunObj;
	
	// Sun variables
	public Flare sunFlare;
	public LightShadows shadowType;
	
	// Skydome and cloud variables
    public float JULIANDATE = 150;
    public float LONGITUDE = 0.0f;
    public float LATITUDE = 0.0f;
    public float MERIDIAN = 0.0f;
    public float TIME = 8.0f;
    public float m_fTurbidity = 2.0f;
    
	public int cloudDensity = 10;
	public GameObject cloudPrefab;
	
	public float cloudSpeed1 = 0.006f;
    public float cloudSpeed2 = 0.002f;
    public float cloudHeight1 = 18.0f;
    public float cloudHeight2 = 41.0f;
    public float cloudTint = 1.9f;
    
	public float m_fRayFactor = 1000.0f;
	public float m_fMieFactor =  0.7f;
    public float m_fDirectionalityFactor = 1.5f;
    public float m_fSunColorIntensity = 0.7f;

	// private variable for Skydome/Cloud calculations
	Vector4 vBetaRayleigh = new Vector4();
    Vector4 vBetaMie = new Vector4();
    Vector3 m_vBetaRayTheta = new Vector3();
    Vector3 m_vBetaMieTheta = new Vector3();
	// private variable for Sun calculations
	Vector3 sunColor = new Vector3();
	float domeRadius = 50f;
    float LATITUDE_RADIANS;
	float LONGITUDE_RADIANS;
	float STD_MERIDIAN;
	
	void Start () {
        sunObj = GameObject.Find("SunLight");
		// Uncomment follwing lines if its the first time you run the script in your Scene
		// Otherwise is really distourbing if you set your 'shadow options' and
		// they get destroyed every now and then... 
	
		if (sunObj == null)
			sunObj= new GameObject("SunLight");
		sunLight = sunObj.GetComponent<Light>();
		if (sunLight == null)
			sunLight = sunObj.AddComponent<Light>();
		
		//sunLight.type = LightType.Directional;
		sunLight.flare = sunFlare;
		sunLight.shadows = shadowType;
		Initialize();
	}
	
	void OnEnable () {
      /*  sunObj = GameObject.Find("SunLight");
		if (sunObj == null)
			sunObj= new GameObject("SunLight");
		
		sunLight = sunObj.GetComponent<Light>();
		if (sunLight == null)
			sunLight = sunObj.AddComponent<Light>();
		
		//sunLight.type = LightType.Directional;
		sunLight.flare = sunFlare;
		sunLight.shadows = shadowType;
		Initialize();*/
	}

	void OnDestroy() {
		//DestroyImmediate(sunObj);	
	}
	
	
	
	// precalculate variables, this has not to be done very frame so we can speed up things.   
	void Initialize() {
		LATITUDE_RADIANS = Mathf.Deg2Rad * LATITUDE;
		LONGITUDE_RADIANS = Mathf.Deg2Rad * LONGITUDE;
		STD_MERIDIAN = MERIDIAN * 15.0f;
		
		
		
	}
	
	void Update()
    {
		initSunThetaPhi();
     	calcAtmosphere();
		
		Vector3 sunLightD = sunLight.transform.TransformDirection(Vector3.forward);
        Vector3 pos = cam.transform.position;
        transform.position = new Vector3(pos.x, 0, pos.z);
        GetComponent<Renderer>().sharedMaterial.SetVector("vBetaRayleigh", vBetaRayleigh);
        GetComponent<Renderer>().sharedMaterial.SetVector("BetaRayTheta", m_vBetaRayTheta);
        GetComponent<Renderer>().sharedMaterial.SetVector("vBetaMie", vBetaMie);                     
        GetComponent<Renderer>().sharedMaterial.SetVector("BetaMieTheta", m_vBetaMieTheta);
        GetComponent<Renderer>().sharedMaterial.SetVector("g_vEyePt",  pos);
        GetComponent<Renderer>().sharedMaterial.SetVector("LightDir", sunLightD);
        GetComponent<Renderer>().sharedMaterial.SetVector("g_vSunColor", sunColor);
        GetComponent<Renderer>().sharedMaterial.SetFloat("DirectionalityFactor", m_fDirectionalityFactor);
        GetComponent<Renderer>().sharedMaterial.SetFloat("SunColorIntensity", m_fSunColorIntensity);
		if(TIME > 5.7&& TIME < 18.1845)
		{
		sunLight.gameObject.active = true;
        GetComponent<Renderer>().sharedMaterial.SetFloat("tint", cloudTint);
		}
		else
		{
		sunLight.gameObject.active = false;
		GetComponent<Renderer>().sharedMaterial.SetFloat("tint", 0);
		}
        GetComponent<Renderer>().sharedMaterial.SetFloat("cloudSpeed1", cloudSpeed1);
        GetComponent<Renderer>().sharedMaterial.SetFloat("cloudSpeed2", cloudSpeed2);
        GetComponent<Renderer>().sharedMaterial.SetFloat("plane_height1", cloudHeight1);
        GetComponent<Renderer>().sharedMaterial.SetFloat("plane_height2", cloudHeight2);
	}
    void calcAtmosphere()
    {
        calcRay();
        CalculateMieCoeff();
    }
	
    void calcRay()
    {
	    const float n  = 1.00029f;		//Refraction index for air
	    const float N  = 2.545e25f;		//Molecules per unit volume
	    const float pn = 0.035f;		//Depolarization factor

        float fRayleighFactor = m_fRayFactor * (Mathf.Pow(Mathf.PI, 2.0f) * Mathf.Pow(n * n - 1.0f, 2.0f) * (6 + 3 * pn)) / ( N * ( 6 - 7 * pn ) );
        
	    m_vBetaRayTheta.x = fRayleighFactor / ( 2.0f * Mathf.Pow( 650.0e-9f, 4.0f ) );
	    m_vBetaRayTheta.y = fRayleighFactor / ( 2.0f * Mathf.Pow( 570.0e-9f, 4.0f ) );
	    m_vBetaRayTheta.z = fRayleighFactor / ( 2.0f * Mathf.Pow( 475.0e-9f, 4.0f ) );

        vBetaRayleigh.x = 8.0f * fRayleighFactor / (3.0f * Mathf.Pow(650.0e-9f, 4.0f));
        vBetaRayleigh.y = 8.0f * fRayleighFactor / (3.0f * Mathf.Pow(570.0e-9f, 4.0f));
        vBetaRayleigh.z = 8.0f * fRayleighFactor / (3.0f * Mathf.Pow(475.0e-9f, 4.0f));
    }
	
    void CalculateMieCoeff()
    {
        float[] K =new float[3];
        K[0]=0.685f;  
        K[1]=0.682f;
        K[2]=0.670f;

	    float c = ( 0.6544f * m_fTurbidity - 0.6510f ) * 1e-16f;	//Concentration factor

	    float fMieFactor = m_fMieFactor * 0.434f * c * 4.0f * Mathf.PI * Mathf.PI;

	    m_vBetaMieTheta.x = fMieFactor / ( 2.0f * Mathf.Pow( 650e-9f, 2.0f ) );
	    m_vBetaMieTheta.y = fMieFactor / ( 2.0f * Mathf.Pow( 570e-9f, 2.0f ) );
	    m_vBetaMieTheta.z = fMieFactor / ( 2.0f * Mathf.Pow( 475e-9f, 2.0f ) );

        vBetaMie.x = K[0] * fMieFactor / Mathf.Pow(650e-9f, 2.0f);
        vBetaMie.y = K[1] * fMieFactor / Mathf.Pow(570e-9f, 2.0f);
        vBetaMie.z = K[2] * fMieFactor / Mathf.Pow(475e-9f, 2.0f);
    }

	// Sunlight calculations
	
	/*void SetPosition ()
	{
		float t = TIME + 0.170f * Mathf.Sin ((4.0f * Mathf.PI * (JULIANDATE - 80.0f)) / 373.0f) - 0.129f * Mathf.Sin ((2.0f * Mathf.PI * (JULIANDATE - 8.0f)) / 355.0f) + (STD_MERIDIAN - LONGITUDE) / 15.0f;
		float fSinT = Mathf.Sin ((Mathf.PI * t) / 12.0f);
		float fCosT = Mathf.Cos ((Mathf.PI * t) / 12.0f);
		
		float solarAltitude = Mathf.Asin (fSinLat * fSinDelta - fCosLat * fCosDelta * fCosT);
		float fTheta = 0f;
		fTheta = Mathf.PI / 2.0f - solarAltitude;
		
		float opp = -fCosDelta * fSinT;
		float adj = -(fCosLat * fSinDelta + fSinLat * fCosDelta * fCosT);
		float SolarAzimuth = Mathf.Atan2 (opp, adj);
		
		float fPhi = Mathf.Atan ((-fCosDelta * fSinT) / (fCosLat * fSinDelta - fSinLat * fCosDelta * fCosT));
		fPhi = -SolarAzimuth;
		
		float fCosTheta = Mathf.Cos (fTheta);
		float fSinTheta = Mathf.Sin (fTheta);
		float fCosPhi = Mathf.Cos (fPhi);
		float fSinPhi = Mathf.Sin (fPhi);
		
		Vector3 m_vDirection = new Vector3 (fSinTheta * fCosPhi, fCosTheta, fSinTheta * fSinPhi);
		float phiSun = (Mathf.PI * 2.0f) - SolarAzimuth;
		
		sunDirection.x = domeRadius;
		sunDirection.y = phiSun;
		sunDirection.z = solarAltitude;
		sunObj.transform.position = sphericalToCartesian (sunDirection);
		
		Vector3 sunDirection2 = calcDirection (fTheta, phiSun);
		sunObj.transform.LookAt (sunDirection2);
		ComputeAttenuation (fTheta);
		
		
	}
	Vector3 calcDirection (float thetaSun, float phiSun)
	{
		Vector3 dir = new Vector3 ();
		dir.x = Mathf.Cos (0.5f * Mathf.PI - thetaSun) * Mathf.Cos (phiSun);
		dir.y = Mathf.Sin (0.5f * Mathf.PI - thetaSun);
		dir.z = Mathf.Cos (0.5f * Mathf.PI - thetaSun) * Mathf.Sin (phiSun);
		return dir.normalized;
	}
	Vector3 sphericalToCartesian (Vector3 sunDir)
	{
		Vector3 res = new Vector3 ();
		res.y = sunDir.x * Mathf.Sin (sunDir.z);
		float tmp = sunDir.x * Mathf.Cos (sunDir.z);
		res.x = tmp * Mathf.Cos (sunDir.y);
		res.z = tmp * Mathf.Sin (sunDir.y);
		return res;
	}*/
	
	void ComputeAttenuation (float m_fTheta)
	{
		float fBeta = 0.04608365822050f * m_fTurbidity - 0.04586025928522f;
		float fTauR, fTauA;
		float[] fTau = new float[3];
		//float tmp = 93.885f - (m_fTheta / Mathf.PI * 180.0f);
		
		float m;
		
		/*if(!(TIME > 5 && TIME < 18))
		{
		m = (float)(1.0f / (Mathf.Cos (m_fTheta) + 0.15f * tmp));
		}
		else
		{*/
		// Relative Optical Mass
		if(TIME > 5.82 && TIME < 18.1)
		{
		cloudTint=1;
		m = (float)(1.0f / (Mathf.Cos(m_fTheta) + 0.15f * Mathf.Pow(93.885f - m_fTheta / Mathf.PI * 180.0f, -1.253f)));
		}
		else
		{
		cloudTint = 0;
		m=20;
		}
		if( m < 0)
		{
			m=20;
		}
		float[] fLambda = new float[3];
		fLambda[0] = 0.65f;	// red (in um.)
		fLambda[1] = 0.57f;	// green (in um.)
		fLambda[2] = 0.475f;// blue (in um.)
		for (int i = 0; i < 3; i++) {
			// Rayleigh Scattering
			// Results agree with the graph (pg 115, MI) 
			// lambda in um.
			fTauR = Mathf.Exp (-m * 0.008735f * Mathf.Pow (fLambda[i], -4.08f));
			
			// Aerosal (water + dust) attenuation
			// beta - amount of aerosols present 
			// alpha - ratio of small to large particle sizes. (0:4,usually 1.3)
			// Results agree with the graph (pg 121, MI) 
			const float fAlpha = 1.3f;
			fTauA = Mathf.Exp (-m * fBeta * Mathf.Pow (fLambda[i], -fAlpha));
			// lambda should be in um
			fTau[i] = fTauR * fTauA;
		}
		
		sunColor = new Vector3 (fTau[0], fTau[1], fTau[2]);
		sunLight.color = new Color (fTau[0], fTau[1], fTau[2]);
		RenderSettings.fogColor = new Color (fTau[0], fTau[1], fTau[2]);;
	}

	
	void initSunThetaPhi() {
	  	float solarDeclination, opp, adj, solarTime, solarAzimuth, solarAltitude;
	  	solarTime = TIME + (0.170f*Mathf.Sin(4f*Mathf.PI*(JULIANDATE - 80f)/373f) - 0.129f*Mathf.Sin(2f*Mathf.PI*(JULIANDATE - 8f)/355f)) + (STD_MERIDIAN - LONGITUDE_RADIANS)/15.0f;
	  	solarDeclination = (0.4093f*Mathf.Sin(2f*Mathf.PI*(JULIANDATE - 81f)/368f));
	  	solarAltitude = Mathf.Asin(Mathf.Sin(LATITUDE_RADIANS) * Mathf.Sin(solarDeclination) -
	    Mathf.Cos(LATITUDE_RADIANS) * Mathf.Cos(solarDeclination) * Mathf.Cos(Mathf.PI*solarTime/12f));
	
	  	opp = -Mathf.Cos(solarDeclination) * Mathf.Sin(Mathf.PI*solarTime/12f);
	  	adj = -(Mathf.Cos(LATITUDE_RADIANS) * Mathf.Sin(solarDeclination) +
		  	Mathf.Sin(LATITUDE_RADIANS) * Mathf.Cos(solarDeclination) *  Mathf.Cos(Mathf.PI*solarTime/12f));
	  	solarAzimuth = Mathf.Atan2(opp, adj);
	
	  	float phiS = -solarAzimuth;
		float thetaS = Mathf.PI / 2.0f - solarAltitude;
		Vector3 sunDirection = new Vector3(domeRadius,phiS, solarAltitude);
		Vector3 sunDirection2 = calcDirection(thetaS,phiS);
		sunObj.transform.position = SphericalToCartesian(sunDirection);
		sunObj.transform.LookAt(sunDirection2);
		ComputeAttenuation(thetaS);
		//sunLight.intensity = _sunIntensity;
		//sunLight.shadowStrength = _shadowIntensity;
	}
	Vector3 calcDirection(float thetaSun, float phiSun)
    {
        Vector3 dir = new Vector3();
        dir.x = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Cos(phiSun);
        dir.y = Mathf.Sin(0.5f * Mathf.PI - thetaSun);
        dir.z = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Sin(phiSun);
        return dir.normalized;
    }
	/// <summary>
    ///   Converts a point from spherical coordinates to cartesian and stores the
    ///   results in the store var.
    /// </summary>
    private static Vector3 SphericalToCartesian(Vector3 sphereCoords)
    {
        Vector3 store;
        store.y = sphereCoords.x*Mathf.Sin(sphereCoords.z);
        float a = sphereCoords.x*Mathf.Cos(sphereCoords.z);
        store.x = a*Mathf.Cos(sphereCoords.y);
        store.z = a*Mathf.Sin(sphereCoords.y);
        return store;
    }
    /// <summary>
    ///   Converts a point from cartesian coordinates to spherical and stores the results in the store var. (Radius, Azimuth, Polar)
    /// </summary>
    private static Vector3 CartesianToSpherical(Vector3 cartCoords)
    {
        Vector3 store;
        if (cartCoords.x == 0)
            cartCoords.x = Mathf.Epsilon;
        store.x = Mathf.Sqrt((cartCoords.x*cartCoords.x) + (cartCoords.y*cartCoords.y) + (cartCoords.z*cartCoords.z));
        store.y = Mathf.Atan(cartCoords.z/cartCoords.x);
        if (cartCoords.x < 0)
            store.y += Mathf.PI;
        store.z = Mathf.Asin(cartCoords.y/store.x);
        return store;
    }
}
