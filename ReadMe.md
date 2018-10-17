<h2>BookieBreaker Fixture Service Stack:</h2>

<h3>BookieBreaker Fixure Service Information</h3>
<p>The BookieBreaker Fixture Service is a single stand alone micro service which is part of the larger 'BookieBreaker' micro service ecosystem.</p>
<p>The service has a sole responsibility for exracting season fixture data and passing this off to the fixture API to be stored in a data repo.</p>
<p>The service is triggered by the creation of new club season associations by way of a BookieBreaker Service Bus.</p>
<p>The Service consists of the following components:
	<ul>
		<li>Fixture API - responsible for managing squad registration data interactions with the underlying data container</li>
		<li>Fixture Extration Svc - responsible for parsing and extracting season participant data</li>
	</ul>
</p>
<p><b>Requires Asp.Net Core 2.1</b></p>