#version 450

#define VELOCITYMAP
#define SHADOWMAP

struct Surface
{
	vec3 pos;
	vec3 wPos;
	vec4 dir;
};

uniform mat4 MVP;
uniform mat4 OldMVP;
uniform float deltaTime;

layout (location = 0) in vec3 position;

out Surface In;
 
void main()
{
	vec4 pos = vec4(position, 1.0);
	vec4 nowPos =  MVP * pos;

	gl_Position = nowPos;
	In.wPos = nowPos.xyz;
	In.pos = position;


#ifdef VELOCITYMAP
	vec4 oldPos = OldMVP * pos;
	In.dir.xy = (nowPos.xy - oldPos.xy);
	In.dir.y *= -1.0;
	In.dir.zw = nowPos.zw;
#endif
}