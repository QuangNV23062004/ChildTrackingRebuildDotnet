using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Ganss.Xss;
using HtmlAgilityPack;

namespace RestAPI.Helpers
{
    public class CloudinaryHelper
    {
        public static async Task<string> UploadImage(
            IFormFile image,
            ICloudinary cloudinary,
            string path
        )
        {
            if (image == null)
            {
                return null;
            }
            var uploadedImage = await cloudinary.UploadAsync(
                new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    PublicId = Guid.NewGuid().ToString(),
                    Folder = path,
                }
            );

            return uploadedImage.Url.ToString();
        }

        public static async Task<List<string>> UploadImages(
            IFormFileCollection images,
            ICloudinary cloudinary,
            string folder
        )
        {
            var uploadedImages = new List<string>();
            if (images == null || images.Count == 0)
            {
                return uploadedImages;
            }

            var uploadTasks = images
                .Select(image => UploadImage(image, cloudinary, folder))
                .ToArray();
            var results = await Task.WhenAll(uploadTasks);

            uploadedImages.AddRange(results.Where(url => url != null));
            return uploadedImages;
        }

        public static async Task<bool> CleanupFile(string path, ICloudinary cloudinary)
        {
            var publicId = path.Split('/').Last();
            var result = await cloudinary.DeleteResourcesAsync(ResourceType.Image, publicId);
            return result.Deleted.Count > 0;
        }

        public static async Task<bool> CleanupFiles(List<string> paths, ICloudinary cloudinary)
        {
            var result = await Task.WhenAll(paths.Select(path => CleanupFile(path, cloudinary)));
            return result.All(r => r);
        }

        public static string FormatBlogContent(string content, List<string> imageUrls)
        {
            // Handle null or empty inputs
            if (string.IsNullOrEmpty(content) || imageUrls == null || !imageUrls.Any())
            {
                return content ?? string.Empty;
            }

            // Step 1: Decode HTML entities and clean up content
            string adjustedContent = DecodeHtmlEntities(content);

            // Step 2: Sanitize the content
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("img");
            sanitizer.AllowedAttributes.Add("src");
            sanitizer.AllowedAttributes.Add("alt");
            sanitizer.AllowedAttributes.Add("width");
            sanitizer.AllowedAttributes.Add("height");
            sanitizer.AllowedAttributes.Add("style");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedCssProperties.Add("display");
            sanitizer.AllowedCssProperties.Add("justify-content");
            sanitizer.AllowedCssProperties.Add("align-items");
            sanitizer.AllowedCssProperties.Add("width");
            sanitizer.AllowedCssProperties.Add("height");
            sanitizer.AllowedCssProperties.Add("max-width");

            string cleanContent = sanitizer.Sanitize(adjustedContent);

            // Step 3: Parse HTML content
            var doc = new HtmlDocument();
            doc.LoadHtml(cleanContent);

            // Step 4: Replace <img> tags with imageUrls in order
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                int imgIndex = 0;
                foreach (var img in images)
                {
                    if (imgIndex >= imageUrls.Count)
                        break;
                    string safeUrl = sanitizer.Sanitize(imageUrls[imgIndex]);
                    img.SetAttributeValue("src", safeUrl);
                    img.SetAttributeValue("alt", $"Blog Image {imgIndex + 1}");
                    img.SetAttributeValue("style", "width: 100%; height: auto; max-width: 800px;");

                    // Optionally wrap in a div for centering
                    var wrapper = doc.CreateElement("div");
                    wrapper.SetAttributeValue(
                        "style",
                        "display: flex; justify-content: center; align-items: center; width: 100%;"
                    );
                    wrapper.InnerHtml = img.OuterHtml;
                    img.ParentNode.ReplaceChild(wrapper, img);

                    imgIndex++;
                }
            }

            // Step 5: Replace [imageN] placeholders with imageUrls in order
            string htmlContent = doc.DocumentNode.OuterHtml;
            for (int i = 0; i < imageUrls.Count; i++)
            {
                string placeholder = $"[image{i + 1}]";
                if (htmlContent.Contains(placeholder))
                {
                    string safeUrl = sanitizer.Sanitize(imageUrls[i]);
                    string imgTag =
                        $"<div style=\"display: flex; justify-content: center; align-items: center; width: 100%;\"><img src=\"{safeUrl}\" alt=\"Blog Image {i + 1}\" style=\"width: 100%; height: auto; max-width: 800px;\" /></div>";
                    htmlContent = htmlContent.Replace(placeholder, imgTag);
                }
            }

            // Step 6: Final sanitize and return
            htmlContent = htmlContent.Replace("\\\"", "\""); // Clean up any remaining encoded quotes
            return sanitizer.Sanitize(htmlContent);
        }

        private static string DecodeHtmlEntities(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // Decode HTML entities
            content = Regex.Replace(
                content,
                @"&#(\d+);",
                match =>
                {
                    int code = int.Parse(match.Groups[1].Value);
                    return ((char)code).ToString();
                }
            );

            content = Regex.Replace(
                content,
                @"&#x([0-9a-fA-F]+);",
                match =>
                {
                    int code = Convert.ToInt32(match.Groups[1].Value, 16);
                    return ((char)code).ToString();
                }
            );

            // Replace encoded characters
            return content
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&apos;", "'")
                .Replace("&#39;", "'")
                .Replace("&nbsp;", " ")
                .Replace("&copy;", "©")
                .Replace("&reg;", "®")
                .Replace("&trade;", "™");
        }

        private static string CleanSrcAttribute(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return src;
            }

            // Clean up quotes and other malformed characters
            src = src.Replace("\\\"", "\"");
            if (src.StartsWith("\"") && src.EndsWith("\""))
            {
                src = src.Substring(1, src.Length - 2);
            }
            if (src.StartsWith("\\\"") && src.EndsWith("\\\""))
            {
                src = src.Substring(2, src.Length - 4);
            }
            if (src.StartsWith("?"))
            {
                src = src.Substring(1);
            }

            return src;
        }
    }
}
