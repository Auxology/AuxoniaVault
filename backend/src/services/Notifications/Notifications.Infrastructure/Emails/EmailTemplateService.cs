using Microsoft.Extensions.Options;

namespace Notifications.Infrastructure.Emails;

public sealed class EmailTemplateService(IOptions<EmailSettings> emailSettings)
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public string CreateLoginEmailTemplate(string email, int token, DateTimeOffset requestedAt)
    {
        var expirationTime = requestedAt.AddMinutes(_settings.TokenExpirationMinutes);
        var formattedTime = expirationTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Your Login Token - {_settings.CompanyName}</title>
    <style>
        body {{
            font-family: var(--font-sans);
            line-height: 1.6;
            color: oklch(0.3211 0 0);
            background-color: oklch(0.9846 0.0017 247.8389);
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            overflow: hidden;
            box-shadow: var(--shadow-lg);
        }}
        .header {{
            background: oklch(0.6231 0.1880 259.8145);
            color: oklch(1.0000 0 0);
            padding: 32px 24px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .content {{
            padding: 32px 24px;
        }}
        .token-container {{
            background-color: oklch(0.9846 0.0017 247.8389);
            border: 2px solid oklch(0.9276 0.0058 264.5313);
            border-radius: var(--radius);
            padding: 24px;
            text-align: center;
            margin: 24px 0;
        }}
        .token {{
            font-size: 32px;
            font-weight: 700;
            color: oklch(0.3211 0 0);
            letter-spacing: 4px;
            font-family: var(--font-mono);
        }}
        .info-box {{
            background-color: oklch(0.9514 0.0250 236.8242);
            border-left: 4px solid oklch(0.6231 0.1880 259.8145);
            padding: 16px;
            margin: 24px 0;
            border-radius: 4px;
        }}
        .info-box p {{
            margin: 0;
            color: oklch(0.3791 0.1378 265.5222);
        }}
        .footer {{
            background-color: oklch(0.9846 0.0017 247.8389);
            padding: 24px;
            text-align: center;
            color: oklch(0.5510 0.0234 264.3637);
            font-size: 14px;
        }}
        .footer a {{
            color: oklch(0.6231 0.1880 259.8145);
            text-decoration: none;
        }}
        .security-notice {{
            background-color: oklch(0.6368 0.2078 25.3313);
            color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .security-notice h3 {{
            color: oklch(1.0000 0 0);
            margin: 0 0 8px 0;
            font-size: 16px;
        }}
        .security-notice p {{
            color: oklch(1.0000 0 0);
            margin: 0;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Your Login Token</h1>
        </div>
        
        <div class=""content"">
            <h2>Hello!</h2>
            <p>You requested a login token for your {_settings.CompanyName} account. Here's your secure login code:</p>
            
            <div class=""token-container"">
                <div class=""token"">{token:D6}</div>
            </div>
            
            <div class=""info-box"">
                <p><strong>⏰ Expires:</strong> {formattedTime}</p>
                <p><strong>📧 Account:</strong> {email}</p>
                <p><strong>🕐 Requested:</strong> {requestedAt:yyyy-MM-dd HH:mm:ss UTC}</p>
            </div>
            
            <div class=""security-notice"">
                <h3>🛡️ Security Notice</h3>
                <p>If you didn't request this login token, please secure your account immediately by revoking all active sessions.</p>
            </div>
            
            <p>This token will expire in {_settings.TokenExpirationMinutes} minutes for your security.</p>
        </div>
        
        <div class=""footer"">
            <p>This email was sent to {email}</p>
            <p>© {DateTime.UtcNow.Year} {_settings.CompanyName}. All rights reserved.</p>
            {(!string.IsNullOrEmpty(_settings.SupportEmail) ? $"<p>Need help? Contact us at <a href=\"mailto:{_settings.SupportEmail}\">{_settings.SupportEmail}</a></p>" : "")}
            {(!string.IsNullOrEmpty(_settings.WebsiteUrl) ? $"<p>Visit our website: <a href=\"{_settings.WebsiteUrl}\">{_settings.WebsiteUrl}</a></p>" : "")}
        </div>
    </div>
</body>
</html>";
    }

    public string CreateEmailChangeVerificationTemplate(string currentEmail, int currentOtp, string ipAddress, string userAgent, DateTimeOffset requestedAt)
    {
        var expirationTime = requestedAt.AddMinutes(_settings.TokenExpirationMinutes);
        var formattedTime = expirationTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Change Verification - {_settings.CompanyName}</title>
    <style>
        body {{
            font-family: var(--font-sans);
            line-height: 1.6;
            color: oklch(0.3211 0 0);
            background-color: oklch(0.9846 0.0017 247.8389);
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            overflow: hidden;
            box-shadow: var(--shadow-lg);
        }}
        .header {{
            background: oklch(0.6368 0.2078 25.3313);
            color: oklch(1.0000 0 0);
            padding: 32px 24px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .content {{
            padding: 32px 24px;
        }}
        .otp-container {{
            background-color: oklch(0.9846 0.0017 247.8389);
            border: 2px solid oklch(0.9276 0.0058 264.5313);
            border-radius: var(--radius);
            padding: 24px;
            text-align: center;
            margin: 24px 0;
        }}
        .otp {{
            font-size: 32px;
            font-weight: 700;
            color: oklch(0.3211 0 0);
            letter-spacing: 4px;
            font-family: var(--font-mono);
        }}
        .info-box {{
            background-color: oklch(0.9514 0.0250 236.8242);
            border-left: 4px solid oklch(0.6231 0.1880 259.8145);
            padding: 16px;
            margin: 24px 0;
            border-radius: 4px;
        }}
        .info-box p {{
            margin: 0;
            color: oklch(0.3791 0.1378 265.5222);
        }}
        .request-details {{
            background-color: oklch(0.9846 0.0017 247.8389);
            border: 1px solid oklch(0.9276 0.0058 264.5313);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .request-details h3 {{
            color: oklch(0.3211 0 0);
            margin: 0 0 12px 0;
            font-size: 16px;
        }}
        .request-details p {{
            color: oklch(0.5510 0.0234 264.3637);
            margin: 4px 0;
            font-size: 14px;
        }}
        .footer {{
            background-color: oklch(0.9846 0.0017 247.8389);
            padding: 24px;
            text-align: center;
            color: oklch(0.5510 0.0234 264.3637);
            font-size: 14px;
        }}
        .footer a {{
            color: oklch(0.6231 0.1880 259.8145);
            text-decoration: none;
        }}
        .security-warning {{
            background-color: oklch(0.6368 0.2078 25.3313);
            color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .security-warning h3 {{
            color: oklch(1.0000 0 0);
            margin: 0 0 8px 0;
            font-size: 16px;
        }}
        .security-warning p {{
            color: oklch(1.0000 0 0);
            margin: 0;
            font-size: 14px;
        }}
        .action-required {{
            background-color: oklch(0.9514 0.0250 236.8242);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .action-required h3 {{
            color: oklch(0.3791 0.1378 265.5222);
            margin: 0 0 8px 0;
            font-size: 16px;
        }}
        .action-required p {{
            color: oklch(0.3791 0.1378 265.5222);
            margin: 0;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📧 Email Change Verification</h1>
        </div>
        
        <div class=""content"">
            <h2>Email Change Request Detected</h2>
            <p>We received a request to change the email address for your {_settings.CompanyName} account. To complete this change, please verify your current email address using the code below:</p>
            
            <div class=""otp-container"">
                <div class=""otp"">{currentOtp:D6}</div>
            </div>
            
            <div class=""info-box"">
                <p><strong>⏰ Expires:</strong> {formattedTime}</p>
                <p><strong>📧 Current Email:</strong> {currentEmail}</p>
                <p><strong>🕐 Requested:</strong> {requestedAt:yyyy-MM-dd HH:mm:ss UTC}</p>
            </div>
            
            <div class=""request-details"">
                <h3>🔍 Request Details</h3>
                <p><strong>IP Address:</strong> {ipAddress}</p>
                <p><strong>User Agent:</strong> {userAgent}</p>
            </div>
            
            <div class=""action-required"">
                <h3>✅ Action Required</h3>
                <p>Enter the verification code above to confirm your email change request. This code will expire in {_settings.TokenExpirationMinutes} minutes.</p>
            </div>
            
            <div class=""security-warning"">
                <h3>⚠️ Security Alert</h3>
                <p>If you did not request this email change, please:</p>
                <ul>
                    <li>Do not use this verification code</li>
                    <li>Immediately revoke all active sessions</li>
                    <li>Contact our support team if needed</li>
                </ul>
            </div>
        </div>
        
        <div class=""footer"">
            <p>This email was sent to {currentEmail}</p>
            <p>© {DateTime.UtcNow.Year} {_settings.CompanyName}. All rights reserved.</p>
            {(!string.IsNullOrEmpty(_settings.SupportEmail) ? $"<p>Need help? Contact us at <a href=\"mailto:{_settings.SupportEmail}\">{_settings.SupportEmail}</a></p>" : "")}
            {(!string.IsNullOrEmpty(_settings.WebsiteUrl) ? $"<p>Visit our website: <a href=\"{_settings.WebsiteUrl}\">{_settings.WebsiteUrl}</a></p>" : "")}
        </div>
    </div>
</body>
</html>";
    }

    public string CreateEmailChangeNewEmailVerificationTemplate(string newEmail, int newOtp, DateTimeOffset requestedAt)
    {
        var expirationTime = requestedAt.AddMinutes(_settings.TokenExpirationMinutes);
        var formattedTime = expirationTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Email Verification - {_settings.CompanyName}</title>
    <style>
        body {{
            font-family: var(--font-sans);
            line-height: 1.6;
            color: oklch(0.3211 0 0);
            background-color: oklch(0.9846 0.0017 247.8389);
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            overflow: hidden;
            box-shadow: var(--shadow-lg);
        }}
        .header {{
            background: oklch(0.6231 0.1880 259.8145);
            color: oklch(1.0000 0 0);
            padding: 32px 24px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .content {{
            padding: 32px 24px;
        }}
        .otp-container {{
            background-color: oklch(0.9846 0.0017 247.8389);
            border: 2px solid oklch(0.9276 0.0058 264.5313);
            border-radius: var(--radius);
            padding: 24px;
            text-align: center;
            margin: 24px 0;
        }}
        .otp {{
            font-size: 32px;
            font-weight: 700;
            color: oklch(0.3211 0 0);
            letter-spacing: 4px;
            font-family: var(--font-mono);
        }}
        .info-box {{
            background-color: oklch(0.9514 0.0250 236.8242);
            border-left: 4px solid oklch(0.6231 0.1880 259.8145);
            padding: 16px;
            margin: 24px 0;
            border-radius: 4px;
        }}
        .info-box p {{
            margin: 0;
            color: oklch(0.3791 0.1378 265.5222);
        }}
        .email-change-info {{
            background-color: oklch(0.9846 0.0017 247.8389);
            border: 1px solid oklch(0.9276 0.0058 264.5313);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .email-change-info h3 {{
            color: oklch(0.3211 0 0);
            margin: 0 0 12px 0;
            font-size: 16px;
        }}
        .email-change-info p {{
            color: oklch(0.5510 0.0234 264.3637);
            margin: 4px 0;
            font-size: 14px;
        }}
        .footer {{
            background-color: oklch(0.9846 0.0017 247.8389);
            padding: 24px;
            text-align: center;
            color: oklch(0.5510 0.0234 264.3637);
            font-size: 14px;
        }}
        .footer a {{
            color: oklch(0.6231 0.1880 259.8145);
            text-decoration: none;
        }}
        .success-notice {{
            background-color: oklch(0.5461 0.2152 262.8809);
            color: oklch(1.0000 0 0);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .success-notice h3 {{
            color: oklch(1.0000 0 0);
            margin: 0 0 8px 0;
            font-size: 16px;
        }}
        .success-notice p {{
            color: oklch(1.0000 0 0);
            margin: 0;
            font-size: 14px;
        }}
        .action-required {{
            background-color: oklch(0.9514 0.0250 236.8242);
            border-radius: var(--radius);
            padding: 16px;
            margin: 24px 0;
        }}
        .action-required h3 {{
            color: oklch(0.3791 0.1378 265.5222);
            margin: 0 0 8px 0;
            font-size: 16px;
        }}
        .action-required p {{
            color: oklch(0.3791 0.1378 265.5222);
            margin: 0;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ New Email Verification</h1>
        </div>
        
        <div class=""content"">
            <h2>Complete Your Email Change</h2>
            <p>Your current email has been verified! Now please verify your new email address to complete the email change process for your {_settings.CompanyName} account.</p>
            
            <div class=""otp-container"">
                <div class=""otp"">{newOtp:D6}</div>
            </div>
            
            <div class=""info-box"">
                <p><strong>⏰ Expires:</strong> {formattedTime}</p>
                <p><strong>📧 New Email:</strong> {newEmail}</p>
                <p><strong>🕐 Requested:</strong> {requestedAt:yyyy-MM-dd HH:mm:ss UTC}</p>
            </div>
            
            <div class=""email-change-info"">
                <h3>📧 Email Change Process</h3>
                <p><strong>Step 1:</strong> ✅ Current email verified</p>
                <p><strong>Step 2:</strong> 🔄 Verify new email (this step)</p>
                <p><strong>Step 3:</strong> ⏳ Email change will be completed</p>
            </div>
            
            <div class=""action-required"">
                <h3>✅ Action Required</h3>
                <p>Enter the verification code above to confirm your new email address. This code will expire in {_settings.TokenExpirationMinutes} minutes.</p>
            </div>
            
            <div class=""success-notice"">
                <h3>🎉 Almost There!</h3>
                <p>You're one step away from completing your email change. Once you verify this new email, your account will be updated.</p>
            </div>
        </div>
        
        <div class=""footer"">
            <p>This email was sent to {newEmail}</p>
            <p>© {DateTime.UtcNow.Year} {_settings.CompanyName}. All rights reserved.</p>
            {(!string.IsNullOrEmpty(_settings.SupportEmail) ? $"<p>Need help? Contact us at <a href=\"mailto:{_settings.SupportEmail}\">{_settings.SupportEmail}</a></p>" : "")}
            {(!string.IsNullOrEmpty(_settings.WebsiteUrl) ? $"<p>Visit our website: <a href=\"{_settings.WebsiteUrl}\">{_settings.WebsiteUrl}</a></p>" : "")}
        </div>
    </div>
</body>
</html>";
    }
}
