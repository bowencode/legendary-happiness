import React, { useState } from 'react';
import { useAuth } from "react-oidc-context";
import Moment from 'moment';

const Notes = () => {
    const auth = useAuth();
    const [notes, setNotes] = useState<{id: string, userId: string, username: string, text: string, updated: Date}[]>();

    React.useEffect(() => {
        (async () => {
            try {
                const token = auth.user?.access_token;
                const response = await fetch("https://localhost:7274/api/note/", {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                });
                setNotes(await response.json());
            } catch (e) {
                console.error(e);
            }
        })();
    }, [auth]);

    if (!notes) {
        return <div>Loading...</div>;
    }

    return (
        <dl>
        {notes.map((note, index) => {
            return <><dt key={index}>{Moment(note.updated).format("MM/DD/yyyy")}</dt><dd>{note.text}</dd></>;
        })}
        </dl>
    );
};

export default Notes;