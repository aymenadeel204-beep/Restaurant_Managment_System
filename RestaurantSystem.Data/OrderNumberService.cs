using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Domain.Models;

namespace RestaurantSystem.Data;

/// <summary>
/// خدمة توليد أرقام الطلبات التسلسلية الموحدة والفريدة لنظام المطعم.
/// تضمن أرقاماً تسلسلية منظمة بصيغة: ORD-000001
/// وتمنع التكرار نهائياً وتمنع إعادة استخدام أرقام طلبات قديمة أو ملغاة.
/// </summary>
public static class OrderNumberService
{
    private static readonly object _syncLock = new();

    /// <summary>
    /// توليد رقم الطلب التسلسلي التالي مع ضمان عدم التكرار وعدم إعادة استخدام أرقام سابقة.
    /// </summary>
    public static string GenerateNextOrderNumber(AppDbContext db)
    {
        lock (_syncLock)
        {
            int maxSequence = 0;

            try
            {
                // 1. فحص جميع أرقام الطلبات الموجودة في جدول الطلبات الحالي
                var existingOrderNumbers = db.Orders
                    .AsNoTracking()
                    .Select(o => o.OrderNumber)
                    .ToList();

                foreach (var num in existingOrderNumbers)
                {
                    int seq = ExtractSequence(num);
                    if (seq > maxSequence) maxSequence = seq;
                }

                // 2. فحص أقصى معرّف ID في الطلبات
                int maxId = db.Orders.AsNoTracking().Select(o => (int?)o.Id).Max() ?? 0;
                if (maxId > maxSequence) maxSequence = maxId;

                // 3. فحص سجلات التدقيق AuditLogs لاكتشاف أي أرقام طلبات تم إلغاؤها أو حذفها
                var auditEntries = db.AuditLogs
                    .AsNoTracking()
                    .Where(a => a.EntityName == "Orders" || a.Description.Contains("ORD-"))
                    .Select(a => a.Description)
                    .ToList();

                foreach (var det in auditEntries)
                {
                    int seq = ExtractSequence(det);
                    if (seq > maxSequence) maxSequence = seq;
                }
            }
            catch
            {
                // في حال حدوث أي استثناء عرضي في القراءة نعتمد على أقصى تسلسل تم الوصول إليه
            }

            // الرقم التسلسلي الجديد
            int nextSeq = maxSequence + 1;
            string candidate = $"ORD-{nextSeq:D6}";

            // التحقق القاطع من عدم وجود هذا الرقم مسبقاً في قاعدة البيانات
            while (db.Orders.Any(o => o.OrderNumber == candidate))
            {
                nextSeq++;
                candidate = $"ORD-{nextSeq:D6}";
            }

            return candidate;
        }
    }

    /// <summary>
    /// استخراج الرقم التسلسلي من النص، سواء كان بصيغة ORD-000001 أو ORD-YYYYMMDD-0001
    /// </summary>
    private static int ExtractSequence(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        // مطابقة نمط ORD-000001 أو ORD-20260918-0001
        var match = Regex.Match(text, @"ORD-(?:\d+-)?(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int val))
        {
            return val;
        }

        // مطابقة أي أرقام بعد كلمة طلب
        var numMatch = Regex.Match(text, @"\b(\d+)\b");
        if (numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out int fallbackVal))
        {
            return fallbackVal;
        }

        return 0;
    }
}
