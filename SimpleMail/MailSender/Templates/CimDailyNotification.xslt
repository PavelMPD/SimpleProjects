<?xml version="1.0" encoding="windows-1251"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>
  <xsl:template match="CimUpdateDataResult">
    <p>
      <b>
        Обработка контрактов должников из CIM :
        <span>
          <xsl:attribute name="style">
            <xsl:value-of select='DebtorsUpdateResult/StatusHtmlStyle'/>
          </xsl:attribute>
          <xsl:value-of select='DebtorsUpdateResult/StatusText'/>
        </span>
      </b>
    </p>
    <p>
      <xsl:value-of select='DebtorsUpdateResult/Message'/>
    </p>
    <p>
      <b>
        Обработка данных об обзвоне должников из CIM :
        <span>
          <xsl:attribute name="style">
            <xsl:value-of select='CallsUpdateResult/StatusHtmlStyle'/>
          </xsl:attribute>
          <xsl:value-of select='CallsUpdateResult/StatusText'/>
        </span>
      </b>
    </p>
    <p>
      <xsl:value-of select='CallsUpdateResult/Message'/>
    </p>
    <p>
      <b>
        Обработка платежей из CIM :
        <span>
          <xsl:attribute name="style">
            <xsl:value-of select='PaymentsUpdateResult/StatusHtmlStyle'/>
          </xsl:attribute>
          <xsl:value-of select='PaymentsUpdateResult/StatusText'/>
        </span>
      </b>
    </p>
    <p>
      <xsl:value-of select='PaymentsUpdateResult/Message'/>
    </p>
  </xsl:template>
</xsl:stylesheet>