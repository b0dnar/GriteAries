var startInterval = function () {
    return setInterval(
        function () {
            $.get("/api/values", function (data) {

                if (data == null) {
                    $('#tableDiv').html('<h2>Лайв вилки відсутні</h2>');
                    return;
                }

                var table_body = '<table border="1" id="example"><thead><tr align="center" height="80"><th width="30">№</th><th width="30">%</th><th width="100">Подія</th><th width="60">Коеф1</th><th width="60">Коеф2</th><th width="60">Коеф3</th><th width="60">Ставка1</th><th width="60">Ставка2</th><th width="60">Ставка3</th><th width="150">Матч</th><th width="200">URL</th></tr></thead><tbody>';
                var temp;

                for (i = 0; i < data.length; i++) {
                    table_body += '<tr align="center" height="80">';

                    table_body += '<td> <p>' + (i+1) + '</p></td>';

                    table_body += '<td> <p>' + data[i].Percent + '</p></td>';

                    table_body += '<td> <p>' + data[i].NameArbitrash + '</p></td>';

                    table_body += '<td> <p>' + data[i].Koef1.Value + '</p>';
                    table_body += '<p>' + data[i].Koef1.BK + '</p></td>';

                    table_body += '<td> <p>' + data[i].Koef2.Value + '</p>';
                    table_body += '<p>' + data[i].Koef2.BK + '</p></td>';

                    if (data[i].State2Event) {
                        table_body += '<td> <p>-</p></td>';
                    } else {
                        table_body += '<td> <p>' + data[i].Koef3.Value + '</p>';
                        table_body += '<p>' + data[i].Koef3.BK + '</p></td>';
                    }
                    
                    table_body += '<td> <p>' + data[i].Stavka1 + '</p></td>';

                    table_body += '<td> <p>' + data[i].Stavka2 + '</p></td>';

                    if (data[i].State2Event) {
                        table_body += '<td> <p>-</p></td>';
                    } else {
                        table_body += '<td> <p>' + data[i].Stavka3 + '</p></td>';
                    }
                    
                    table_body += '<td> <p>' + data[i].NameMatches + '</p></td>';

                    table_body += '<td>';
                    //table_body += '<p>' + data[i].Urls[0] + '</p>';
                    //table_body += '<p>' + data[i].Urls[1] + '</p>';
                    for (var j = 0; j < data[i].Urls.length; j++) {
                        table_body += '<p>' + data[i].Urls[j] + '</p>';
                    }
                    table_body += '</td>';

                    table_body += '</tr>';
                }

                table_body += '</tbody></table>';
                $('#tableDiv').html(table_body);

                
            });
        }, 10000);
}

var timer = startInterval();

