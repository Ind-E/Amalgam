shader_type canvas_item;

// The texture that will be distorted.
uniform sampler2D portal_texture : source_color, filter_linear_mipmap_anisotropic, repeat_enable;

uniform vec4 portal_tint : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float vortex_effect_radius : hint_range(0.01, 1.0, 0.01) = 0.45;
uniform float twist_strength : hint_range(0.0, 30.0, 0.1) = 8.0;
uniform float pulsation_speed : hint_range(0.0, 5.0, 0.01) = 0.7;
uniform float breath_magnitude : hint_range(-0.3, 0.3, 0.005) = 0.05;
uniform float overall_rotation_speed : hint_range(-2.0, 2.0, 0.01) = 0.25;
uniform float texture_scroll_speed_x : hint_range(-1.0, 1.0, 0.01) = 0.08;
uniform float texture_scroll_speed_y : hint_range(-1.0, 1.0, 0.01) = 0.03;
uniform float edge_softness : hint_range(0.01, 0.5, 0.005) = 0.1;

mat2 rotate(float angle) {
    float s = sin(angle);
    float c = cos(angle);
    return mat2(vec2(c, -s), vec2(s, c));
}

void fragment() {
    // 1. UV Preparation and Aspect Correction
    vec2 node_pixel_size = 1.0 / TEXTURE_PIXEL_SIZE;
    vec2 uv = UV - 0.5;

    if (node_pixel_size.x > node_pixel_size.y) {
        uv.x *= node_pixel_size.x / node_pixel_size.y;
    } else {
        uv.y *= node_pixel_size.y / node_pixel_size.x;
    }

    // 2. Pulsation
    float time_progress = sin(TIME * pulsation_speed);

    // 3. Overall Rotation
    uv = rotate(TIME * overall_rotation_speed) * uv;

    // 4. Distance Calculation
    float dist = length(uv);

    // 5. Vortex/Twist
    // We calculate twist based on distance before we distort the UVs further
    float twist_spatial_factor = smoothstep(vortex_effect_radius, 0.0, dist);
    float twist_angle = twist_spatial_factor * (twist_strength * time_progress);
    uv = rotate(twist_angle) * uv;

    // 6. Breathing Effect
    float breath = 1.0 + (breath_magnitude * time_progress);
    uv *= breath;

    // 7. Texture Scrolling
    vec2 scroll = TIME * vec2(texture_scroll_speed_x, texture_scroll_speed_y);
    vec2 final_uv = uv + scroll + 0.5; // Centering back for sampling

    // 8. Alpha Mask (Crucial for fixing edge artifacts)
    // This ensures the portal is perfectly circular and fades before the quad edge
    float alpha_mask = smoothstep(vortex_effect_radius, vortex_effect_radius - edge_softness, dist);

    // 9. Sampling
    vec4 tex = texture(portal_texture, final_uv);

    COLOR = tex * portal_tint;
    COLOR.a *= alpha_mask;

}