import os
import cv2
import numpy as np

def generate_text_image(
    text,
    image_size=(512, 512), background_color=(255, 255, 255),
    font_scale=10, text_color=(0, 0, 0),
    border=False, border_color=(0, 0, 0), border_thickness=5
):
    image = np.ones((image_size[1], image_size[0], 3), dtype=np.uint8)
    image[:] = background_color
    
    font = cv2.FONT_HERSHEY_SIMPLEX
    (text_width, text_height), baseline = cv2.getTextSize(text, font, font_scale, thickness=10)
    x = (image.shape[1] - text_width) // 2
    y = (image.shape[0] + text_height) // 2
    cv2.putText(image, text, (x, y), font, font_scale, text_color, thickness=10)

    if not border:
        return image
    cv2.rectangle(
        image, (border_thickness // 2, border_thickness // 2), 
        (image_size[0] - border_thickness // 2, image_size[1] - border_thickness // 2), 
        border_color, thickness=border_thickness
    )
    return image;

def generate_nxn_images(
    n,
    mipmap_level,
    save_dir="generated_images",
    image_size=(512, 512),
    font_scale=3,
    text_color=(0, 0, 0),
    background_color=(255, 255, 255),
    border=False, border_color=(0, 0, 0), border_thickness=5
):
    for i in range(n):
        for j in range(n):
            text = f"{mipmap_level}-{i}-{j}"
            save_path = os.path.join(save_dir, f"{text}.png")
            image = generate_text_image(
                text=text,
                image_size=image_size,
                background_color=background_color,
                font_scale=font_scale,
                text_color=text_color,
                border=border,
                border_color=border_color,
                border_thickness=border_thickness
            )
            cv2.imwrite(save_path, image)

if __name__ == '__main__':
    n = 1
    colors = [
        (0, 0, 0),
        (0, 0, 255),
        (0, 255, 0),
        (255, 0, 0)
    ]
    for mipmap_level in range(len(colors)):
        generate_nxn_images(
            n=n,
            mipmap_level=mipmap_level,
            font_scale=4,
            save_dir='.',
            border=True,
            border_color=(0, 0, 0),
            text_color=colors[mipmap_level]
        )
        n *= 2