const express = require('express');
const puppeteer = require('puppeteer');

const app = express();
app.use(express.json({ limit: '50mb' })); // Increase limit for large HTML content
const PORT = process.env.PORT || 3000;

app.post('/convert-to-pdf', async (req, res) => {
    const { html } = req.body;

    if (!html) {
        return res.status(400).send('HTML content is required.');
    }

    let browser;
    try {
        browser = await puppeteer.launch({
            executablePath: '/usr/bin/chromium', // Specify the path to the installed Chromium
            headless: true, // Use 'new' for new headless mode, 'true' for old
            args: [
                '--no-sandbox',
                '--disable-setuid-sandbox',
                '--disable-dev-shm-usage',
                '--disable-accelerated-2d-canvas',
                '--no-first-run',
                '--no-zygote',
                // '--single-process', // This might or might not help with the /dev/shm issue - often not needed with explicit executablePath
                '--disable-gpu'
            ]
        });
        const page = await browser.newPage();

        // Set content and wait for network activity to be idle
        await page.setContent(html, {
            waitUntil: 'networkidle0', // Wait until there are no more than 0 network connections for at least 500 ms
        });

        // Generate PDF
        const pdf = await page.pdf({
            format: 'A4',
            printBackground: true, // Ensure background colors/images are printed
            margin: {
                top: '20mm',
                right: '20mm',
                bottom: '20mm',
                left: '20mm'
            }
        });

        res.contentType('application/pdf');
        res.send(pdf);
    } catch (error) {
        console.error('Error generating PDF:', error);
        res.status(500).json({ detail: `Failed to generate PDF: ${error.message}` });
    } finally {
        if (browser) {
            await browser.close();
        }
    }
});

app.get('/health', (req, res) => {
    res.status(200).send('Puppeteer service is healthy');
});

app.listen(PORT, () => {
    console.log(`Puppeteer service listening on port ${PORT}`);
});
