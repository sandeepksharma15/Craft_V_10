using System.ComponentModel.DataAnnotations;
using Craft.Security;

namespace Craft.AppComponents.Security;

/// <summary>
/// Client-side form model for the <see cref="RegisterUser{TUser}"/> component.
/// Includes a <see cref="ConfirmPassword"/> field for in-browser validation that
/// is never sent to the server.
/// </summary>
public sealed class RegisterFormModel : ICreateUserRequest
{
    /// <summary>Gets or sets the user's first name.</summary>
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    /// <summary>Gets or sets the user's e-mail address.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    /// <summary>Gets or sets the desired password.</summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    /// <summary>Gets or sets the password confirmation (client-only; not mapped to the server model).</summary>
    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }
}
