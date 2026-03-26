shader_type canvas_item;

uniform float seed = 1.0;

uniform vec3 rust_color_1 : source_color = vec3(0.35, 0.15, 0.05);
uniform vec3 rust_color_2 : source_color = vec3(0.5, 0.25, 0.1);
uniform vec3 metal_color : source_color = vec3(0.475, 0.475, 0.475);

uniform float rust_threshold : hint_range(0.0, 1.0) = 0.55;
uniform float rust_scale : hint_range(1.0, 50.0) = 12.0;
uniform float rust_spread : hint_range(0.0, 1.0) = 0.12;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

const int NOISE_OCTAVES = 5;

float hash(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

float noise(vec2 x) {
    vec2 i = floor(x);
    vec2 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));

    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
    float value = 0.0;
    float amplitude = 0.5;
    for (int i = 0; i < NOISE_OCTAVES; i++) {
        value += amplitude * noise(p);
        p *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void fragment() {
    vec4 col = textureLod(screen_texture, SCREEN_UV, 0.0);

    float avg = (col.r + col.g + col.b) / 3.0;

    vec2 seed_offset = vec2(seed * 1.432, seed * 5.678);

    float noise = fbm((UV + seed_offset) * rust_scale);

    float rust_mask = smoothstep(rust_threshold - rust_spread, rust_threshold + rust_spread, noise);

    vec3 rust_gradient = mix(rust_color_1, rust_color_2, noise);

    COLOR = vec4(mix(metal_color * avg, rust_gradient * avg, rust_mask), col.a);
}
