uniform extern texture ScreenTexture;    
uniform extern bool drawLeftLens = true;

float distK0;
float distK1;
float distK2;
float distK3;
float imageScaleFactor;

sampler ScreenS = sampler_state
{
    Texture = <ScreenTexture>;    
};

float4 OculusPixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR
{
    float lensOffset = -0.0635;
    float2 lensCenter;
    float2 scaleIn;
    float2 scale;
    float xShift = -0.08;    
    float aspectRatio = 0.8f;
    float scaleFactor = 0.5f;//0.5/2.0;
    float scaleInFactor = 2.0f;//2.0/0.5;
    if (drawLeftLens)
    {
        lensCenter.x = 0.5f + lensOffset * 0.5;
        texCoord.x = texCoord.x + xShift;
    }
    else
    {
        lensCenter.x = 0.5f - lensOffset * 0.5;
        texCoord.x = texCoord.x - xShift;
    }
    lensCenter.y = 0.5;
    scaleFactor = 0.5f;//1.0/2.0;
    scaleInFactor = 2.0f;//2.0/1.0;
    scale.x = scaleFactor * imageScaleFactor;
    scale.y = scaleFactor * aspectRatio * imageScaleFactor;
    scaleIn.x = scaleInFactor;
    scaleIn.y = scaleInFactor / aspectRatio;
    float2 theta = (texCoord - lensCenter ) * scaleIn;
    float rSq = pow(theta.x ,2) + pow (theta.y, 2);
    float2 rvector= theta * (distK0 + distK1 * rSq + distK2 * pow(rSq,2)  +  distK3 * pow(rSq,3) );
    float2 newVector =  lensCenter + scale * rvector;
    if (newVector.x < 0.0 || newVector.x > 1.0 || newVector.y < 0.0 || newVector.y > 1.0)
    {
        return float4(0.0,0.0,0.0,1.0);
    }
    else
    {
        float4 target = tex2D(ScreenS, newVector);
        return float4(target.r * 1.0f, target.g *1.0f, target.b * 1.0f, 1.0f); 
    }
}

technique Technique1  
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 OculusPixelShaderFunction();
    }
}
