INSERT INTO transaktion
(betrag, beschreibung, istWiederkehrend, user_id, kategorie_id, datum)
VALUES

-- Ausgaben
(49.90, 'Monatsticket', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Transport'),
 '2025-01-01 08:00:00'),

(85.40, 'Wocheneinkauf', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Lebensmittel'),
 '2025-01-05 17:30:00'),

(1200.00, 'Wohnungsmiete Januar', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Miete'),
 '2025-01-03 09:00:00'),

(15.99, 'Netflix', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Freizeit'),
 '2025-01-07 20:00:00'),

(89.50, 'KFZ Versicherung', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Versicherungen'),
 '2025-01-10 10:00:00'),

(32.75, 'Restaurantbesuch', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Freizeit'),
 '2025-01-14 19:45:00'),

(18.90, 'Apotheke', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Sonstiges (Ausgabe)'),
 '2025-01-17 12:20:00'),

-- Einnahmen
(3200.00, 'Januargehalt', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Gehalt'),
 '2025-01-01 00:00:00'),

(450.00, 'Website für Kunden', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Freelancing'),
 '2025-01-12 16:00:00'),

(125.50, 'ETF Ausschüttung', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Investitionen'),
 '2025-01-20 10:00:00'),

(14.25, 'Tagesgeldzinsen', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Zinsen'),
 '2025-01-31 23:59:59'),

(80.00, 'Verkauf alter Monitor', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Verkauf'),
 '2025-01-22 14:15:00'),

-- Januar 2026
(3350.00, 'Januargehalt', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Gehalt'),
 '2026-01-01 00:00:00'),

(1225.00, 'Wohnungsmiete Januar', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Miete'),
 '2026-01-03 09:00:00'),

(92.30, 'Wocheneinkauf', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Lebensmittel'),
 '2026-01-05 18:10:00'),

(54.90, 'Deutschlandticket', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Transport'),
 '2026-01-06 08:00:00'),

(16.99, 'Streaming Abo', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Freizeit'),
 '2026-01-08 20:00:00'),

(500.00, 'Nebenprojekt', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Freelancing'),
 '2026-01-15 15:30:00'),

-- Februar 2026
(3350.00, 'Februargehalt', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Gehalt'),
 '2026-02-01 00:00:00'),

(1225.00, 'Wohnungsmiete Februar', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Miete'),
 '2026-02-03 09:00:00'),

(87.15, 'Wocheneinkauf', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Lebensmittel'),
 '2026-02-07 17:45:00'),

(24.90, 'Kinoabend', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Freizeit'),
 '2026-02-12 21:00:00'),

(135.80, 'ETF Ausschüttung', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Investitionen'),
 '2026-02-18 11:15:00'),

-- März 2026
(3350.00, 'Märzgehalt', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Gehalt'),
 '2026-03-01 00:00:00'),

(1225.00, 'Wohnungsmiete März', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Miete'),
 '2026-03-03 09:00:00'),

(95.60, 'Großeinkauf', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Lebensmittel'),
 '2026-03-06 18:20:00'),

(210.00, 'Verkauf Fahrrad', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Verkauf'),
 '2026-03-10 14:00:00'),

(89.50, 'KFZ Versicherung', 1, 1,
 (SELECT id FROM kategorie WHERE name = 'Versicherungen'),
 '2026-03-15 09:30:00'),

(19.75, 'Tagesgeldzinsen Q1', 0, 1,
 (SELECT id FROM kategorie WHERE name = 'Zinsen'),
 '2026-03-31 23:59:59');